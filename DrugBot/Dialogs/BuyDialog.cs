using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using DrugBot.Data;
using Microsoft.Bot.Connector;
using DrugBot.Common;

namespace DrugBot.Dialogs
{
    [Serializable]
    public class BuyDialog : BaseDialog, IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            // get drugs/random prices
            var db = new DrugBotDataContext();
            var drugs = db.GetDrugs().ToList();

            // check state data for existing drug prices and dont overwrite these location's prices
            var drugPrices = this.GetDrugPrices(context);

            // setup buttons
            var buttons = new List<CardAction>();
            foreach(var drugPrice in drugPrices)
            {
                var drug = drugs.Single(x => x.DrugId == drugPrice.Key);

                buttons.Add(new CardAction
                {
                    Title = $"{drug.Name}: {drugPrice.Value:C0}",
                    Type = ActionTypes.ImBack,
                    Value = drug.Name,
                });
            }

            this.AddCancelButton(buttons);

            await context.PostAsync(this.SetupHeroResponse(context, buttons, "What do you want to buy?"));

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
                var db = new DrugBotDataContext();
                var drugs = db.GetDrugs().ToList()
                    .Select(x => new
                    {
                        Name = x.Name.ToLower(),
                    });

                if (drugs.Any(x => x.Name == message.Text.ToLower()))
                {
                    // send intended drug to state
                    // confirm db record matches, so we store a good drug name to bot state
                    var drug = drugs.Single(x => x.Name == message.Text.ToLower());
                    context.UserData.SetValue(StateKeys.DrugToBuy, drug.Name);

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

                var userId = context.UserData.Get<int>(StateKeys.UserId);

                // todo: put this somewhere common
                var db = new DrugBotDataContext();
                var drugs = db.GetDrugs().ToList()
                    .Select(x => new
                    {
                        Name = x.Name,
                        NameLower = x.Name.ToLower(),
                        DrugId = x.DrugId,
                    });

                var user = db.Users.FirstOrDefault(x => x.UserId == userId);

                if(user != null)
                {
                    // determine drug price
                    var drugPrices = this.GetDrugPrices(context);
                    var drugToBuy = context.UserData.Get<string>(StateKeys.DrugToBuy);
                    var drug = drugs.Single(x => x.NameLower == drugToBuy);
                    var price = drugPrices[drug.DrugId];

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
                                DrugId = drug.DrugId,
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