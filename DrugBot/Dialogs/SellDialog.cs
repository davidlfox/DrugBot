using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using DrugBot.Data;
using DrugBot.Common;

namespace DrugBot.Dialogs
{
    [Serializable]
    public class SellDialog : BaseDialog, IDialog<GameState>
    {
        public async Task StartAsync(IDialogContext context)
        {
            var db = new DrugBotDataContext();

            // check inventory to see if they have anything to sell
            var userId = context.UserData.Get<int>(StateKeys.UserId);
            var user = db.Users.Single(x => x.UserId == userId);

            if (!user.Inventory.Any(x => x.Quantity > 0))
            {
                await context.PostAsync("You don't have anything to sell...");
                this.Done(context);
            }
            else
            {
                var buttons = this.GetDrugButtons(context);

                this.AddCancelButton(buttons);

                await context.PostAsync(this.SetupHeroResponse(context, buttons, "What do you want to sell?"));

                context.Wait(MessageReceivedAsync);
            }
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
                        Name = x.Name.ToLower(),
                        DrugId = x.DrugId,
                    });

                var drugPrices = this.GetDrugPrices(context);

                if (drugs.Any(x => x.Name == message.Text.ToLower()))
                {
                    var drug = drugs.Single(x => x.Name == message.Text.ToLower());

                    var user = this.GetUser(context);
                    var inventory = user.Inventory.FirstOrDefault(x => x.DrugId == drug.DrugId);

                    if (inventory != null && inventory.Quantity > 0)
                    {
                        if (drugPrices.Any(x => x.Key == drug.DrugId))
                        {
                            context.UserData.SetValue(StateKeys.DrugToSell, drug.Name);

                            // get inventory
                            var qty = user.Inventory.Single(x => x.DrugId == drug.DrugId).Quantity;

                            // prompt for quantity
                            PromptDialog.Number(context, SellQuantityAsync, $"You have {qty:n0}. How much do you want to sell?");
                        }
                    }
                    else
                    {
                        await context.PostAsync("You don't have any of that. Type CANCEL if you don't want to sell anything.");
                        context.Wait(MessageReceivedAsync);
                    }
                }
                else
                {
                    await context.PostAsync("You can't sell that...Type CANCEL if you don't want to sell anything.");
                    context.Wait(MessageReceivedAsync);
                }
            }
        }

        private async Task SellQuantityAsync(IDialogContext context, IAwaitable<long> result)
        {
            var qty = await result;
            if (qty < 1)
            {
                await context.PostAsync("Looks like you don't want to sell any--thanks for wasting my time");
                this.Done(context);
            }
            else
            {
                // yeah, i know this could truncate
                var quantity = Convert.ToInt32(qty);
                var db = new DrugBotDataContext();

                var userId = context.UserData.Get<int>(StateKeys.UserId);
                var user = db.Users.Single(x => x.UserId == userId);

                var drugs = db.GetDrugs().ToList()
                    .Select(x => new
                    {
                        Name = x.Name,
                        NameLower = x.Name.ToLower(),
                        DrugId = x.DrugId,
                    });

                // determine drug price
                var drugPrices = this.GetDrugPrices(context);
                var drugToSell = context.UserData.Get<string>(StateKeys.DrugToSell);
                var drug = drugs.Single(x => x.NameLower == drugToSell);

                if (user.Inventory.Any(x => x.DrugId == drug.DrugId && x.Quantity >= quantity))
                {
                    var price = drugPrices[drug.DrugId];

                    var total = price * quantity;
                    user.Wallet += total;
                    var item = user.Inventory.Single(x => x.DrugId == drug.DrugId);
                    item.Quantity -= quantity;

                    try
                    {
                        db.Commit();
                    }
                    catch
                    {
                        await context.PostAsync("Something happened when saving your sell inventory. Yeah, I'm still an alpha bot...");
                    }

                    await context.PostAsync($"You sold {qty} for {total:C0}.");
                    await context.PostAsync($"You have {user.Wallet:C0} in your wallet.");
                    this.Done(context);
                }
                else
                {
                    await context.PostAsync("You don't have that much to sell...");
                    this.Done(context);
                }
            }
        }
    }
}