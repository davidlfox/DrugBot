using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using DrugBot.Data;
using Microsoft.Bot.Connector;

namespace DrugBot.Dialogs
{
    [Serializable]
    public class BuyDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            // get location of user--how? we dont have user id. bot state?
            var locationId = context.UserData.Get<int>("LocationId");

            if (locationId == 0)
            {
                throw new NotImplementedException();
            }

            // get drugs/random prices
            var db = new DrugBotDataContext();
            var drugs = db.Drugs.ToList();

            // todo: check state data for existing drug prices and dont overwrite these location prices

            var drugPrices = new Dictionary<string, int>();

            var rand = new Random();
            var multiplier = 1;

            List<CardAction> buttons = new List<CardAction>();

            foreach (var drug in drugs)
            {
                var price = rand.Next(5, 20) * multiplier;
                drugPrices.Add(drug.Name, price);
                buttons.Add(new CardAction
                {
                    Title = $"{drug.Name}: {price:C0}",
                    Type = "imBack",
                    Value = drug.Name,
                });
            }

            // setup hero card
            HeroCard heroCard = new HeroCard
            {
                Buttons = buttons,
                Text = "What do you want to buy?"
            };

            // setup attachment
            var attachment = heroCard.ToAttachment();

            // send hero card to user
            var activity = context.MakeMessage();
            activity.Attachments = new List<Attachment>();
            activity.Attachments.Add(attachment);

            await context.PostAsync(activity);

            // store to state
            context.UserData.SetValue("drugPrices", drugPrices);

            // wait for drug selection
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            var drugPrices = context.UserData.Get< Dictionary<string, int>>("drugPrices");

            if (drugPrices.ContainsKey(message.Text.ToLower()))
            {
                // send intended drug to state
                context.UserData.SetValue("DrugToBuy", message.Text.ToLower());

                // prompt for quantity
                PromptDialog.Number(context, BuyQuantityAsync, "How much do you want to buy?");
            }
            else
            {
                await context.PostAsync("That doesn't sound like a drug that's available here");
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task BuyQuantityAsync(IDialogContext context, IAwaitable<long> result)
        {
            var qty = await result;

            // todo: make enum or static key names
            var userId = context.UserData.Get<int>("UserId");

            // todo: put this somewhere common
            var db = new DrugBotDataContext();
            var user = db.Users.FirstOrDefault(x => x.UserId == userId);

            if(user != null)
            {
                // determine drug price
                var drugPrices = context.UserData.Get<Dictionary<string, int>>("drugPrices");
                var drugToBuy = context.UserData.Get<string>("DrugToBuy").ToLower();
                var price = drugPrices[drugToBuy];

                // check wallet for enough money
                if (user.Wallet >= price * qty)
                {
                    var cost = price * Convert.ToInt32(qty);

                    // do transaction
                    user.Wallet -= cost;
                    await context.PostAsync($"You spent {cost:C0} on {qty} units of {drugToBuy}");
                    // todo: add inventory 

                    context.Done<object>(null);
                }
                else
                {
                    await context.PostAsync("You don't have enough money to buy that.");
                    context.Done<object>(null);
                }
            }
        }
    }
}