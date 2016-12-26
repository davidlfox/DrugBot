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
    public class BuyDialog : BaseDialog, IDialog<GameState>
    {
        public async Task StartAsync(IDialogContext context)
        {
            // setup buttons
            var buttons = this.GetDrugButtons(context);

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
                this.Done(context);
            }
            else
            {
                var db = new DrugBotDataContext();
                var drugs = db.GetDrugs().ToList()
                    .Select(x => new
                    {
                        DrugId = x.DrugId,
                        Name = x.Name.ToLower(),
                    });

                if (drugs.Any(x => x.Name == message.Text.ToLower()))
                {
                    // send intended drug to state
                    // confirm db record matches, so we store a good drug name to bot state
                    var drug = drugs.Single(x => x.Name == message.Text.ToLower());
                    context.UserData.SetValue(StateKeys.DrugToBuy, drug.Name);

                    // get affordability
                    var user = this.GetUser(context);
                    var drugPrice = this.GetDrugPrices(context).Single(x => x.Key == drug.DrugId).Value;
                    var canAfford = user.Wallet / drugPrice;

                    // prompt for quantity
                    PromptDialog.Number(context, BuyQuantityAsync, $"You can afford {canAfford:n0}. How much do you want to buy?");
                }
                else
                {
                    await context.PostAsync("You can't buy that...Type CANCEL if you don't want to buy anything.");
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
                this.Done(context);
            }
            else
            {
                var quantity = Convert.ToInt32(qty);

                // todo: put this somewhere common
                var db = new DrugBotDataContext();
                var drugs = db.GetDrugs().ToList()
                    .Select(x => new
                    {
                        Name = x.Name,
                        NameLower = x.Name.ToLower(),
                        DrugId = x.DrugId,
                    });

                var userId = context.UserData.Get<int>(StateKeys.UserId);
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

                        this.Done(context);
                    }
                    else
                    {
                        await context.PostAsync("You don't have enough money to buy that.");
                        this.Done(context);
                    }
                }
            }
        }
    }
}