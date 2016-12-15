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

            // check state data for existing drug prices and dont overwrite these location prices
            var drugPrices = new Dictionary<string, int>();

            if (!context.UserData.TryGetValue("drugPrices", out drugPrices))
            {
                drugPrices = new Dictionary<string, int>();

                var rand = new Random();

                foreach (var drug in drugs)
                {
                    var price = rand.Next(drug.MinPrice, drug.MaxPrice);
                    drugPrices.Add(drug.Name, price);
                }

                // store to state
                context.UserData.SetValue("drugPrices", drugPrices);
            }

            // setup buttons
            var buttons = new List<CardAction>();
            foreach(var drug in drugPrices)
            {
                buttons.Add(new CardAction
                {
                    Title = $"{drug.Key}: {drug.Value:C0}",
                    Type = ActionTypes.ImBack,
                    Value = drug.Key,
                });
            }

            buttons.Add(new CardAction
            {
                Title = "Cancel",
                Type = ActionTypes.ImBack,
                Value = "Cancel",
            });

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

            // wait for drug selection
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (message.Text.ToLower() == "cancel")
            {
                context.Done<object>(null);
            }
            else
            {
                var drugPrices = context.UserData.Get< Dictionary<string, int>>("drugPrices");

                if (drugPrices.ContainsKey(message.Text) || drugPrices.ContainsKey(message.Text.ToLower()))
                {
                    // send intended drug to state
                    // confirm db record matches, so we store a good drug name to bot state
                    var db = new DrugBotDataContext();
                    var drug = db.Drugs.Single(x => x.Name == message.Text);
                    context.UserData.SetValue("DrugToBuy", drug.Name);

                    // prompt for quantity
                    PromptDialog.Number(context, BuyQuantityAsync, "How much do you want to buy?");
                }
                else
                {
                    await context.PostAsync("That doesn't sound like a drug that's available here");
                    context.Wait(MessageReceivedAsync);
                }
            }
        }

        private async Task BuyQuantityAsync(IDialogContext context, IAwaitable<long> result)
        {
            var qty = await result;
            // yeah, i know this could truncate
            if(qty < 1)
            {
                await context.PostAsync("Looks like you don't want to buy any--thanks for wasting my time");
                context.Done<object>(null);
            }
            else
            {
                var quantity = Convert.ToInt32(qty);

                // todo: make enum or static key names
                var userId = context.UserData.Get<int>("UserId");

                // todo: put this somewhere common
                var db = new DrugBotDataContext();
                var user = db.Users.FirstOrDefault(x => x.UserId == userId);

                if(user != null)
                {
                    // determine drug price
                    var drugPrices = context.UserData.Get<Dictionary<string, int>>("drugPrices");
                    var drugToBuy = context.UserData.Get<string>("DrugToBuy");
                    var drug = db.Drugs.Single(x => x.Name == drugToBuy);
                    var price = drugPrices[drugToBuy];

                    // check wallet for enough money
                    if (user.Wallet >= price * qty)
                    {
                        var cost = price * quantity;

                        // do transaction
                        user.Wallet -= cost;
                        await context.PostAsync($"You spent {cost:C0} on {qty} units of {drug.Name}");
                        await context.PostAsync($"You have {user.Wallet:C0} remaining");

                        // add inventory 
                        if (user.Inventory.Any(x => x.Drug.Name == drug.Name))
                        {
                            // already has zero or more of this drug, add to it
                            var inventory = user.Inventory.FirstOrDefault(x => x.Drug.Name == drug.Name);
                            inventory.Quantity += quantity;
                        }
                        else
                        {
                            // add this inventory for the firs time
                            var inventory = new InventoryItem
                            {
                                User = user,
                                Drug = drug,
                                Quantity = quantity,
                            };
                            user.Inventory.Add(inventory);
                        }

                        db.SaveChanges();

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
}