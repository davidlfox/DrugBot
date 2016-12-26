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
    public class SellDialog : BaseDialog, IDialog<object>
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
                context.Done<object>(null);
            }
            else
            {
                var drugs = db.GetDrugs().ToList();

                // display drug prices
                var drugPrices = this.GetDrugPrices(context);

                var buttons = new List<CardAction>();
                foreach (var drugPrice in drugPrices)
                {
                    var drug = drugs.Single(x => x.DrugId == drugPrice.Key);

                    buttons.Add(new CardAction
                    {
                        Title = $"{drug.Name} ({drugPrice:C0})",
                        Type = ActionTypes.ImBack,
                        Value = drug.Name,
                    });
                }

                this.AddCancelButton(buttons);

                // setup hero card
                HeroCard heroCard = new HeroCard
                {
                    Buttons = buttons,
                    Text = "What do you want to sell?"
                };

                var attachment = heroCard.ToAttachment();

                var activity = context.MakeMessage();
                activity.Attachments = new List<Attachment>();
                activity.Attachments.Add(attachment);

                await context.PostAsync(activity);

                context.Wait(MessageReceivedAsync);
            }
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
                        DrugId = x.DrugId,
                    });

                var drugPrices = this.GetDrugPrices(context);

                if (drugs.Any(x => x.Name == message.Text.ToLower()))
                {
                    var drug = drugs.Single(x => x.Name == message.Text.ToLower());

                    if (drugPrices.Any(x => x.Key == drug.DrugId))
                    {
                        context.UserData.SetValue(StateKeys.DrugToSell, drug.Name);

                        // prompt for quantity
                        PromptDialog.Number(context, SellQuantityAsync, "How much do you want to sell?");
                    }
                    else
                    {
                        await context.PostAsync("You can't sell that here...");
                        context.Wait(MessageReceivedAsync);
                    }
                }
            }
        }

        private async Task SellQuantityAsync(IDialogContext context, IAwaitable<long> result)
        {
            var qty = await result;
            if (qty < 1)
            {
                await context.PostAsync("Looks like you don't want to sell any--thanks for wasting my time");
                context.Done<object>(null);
            }
            else
            {
                // yeah, i know this could truncate
                var quantity = Convert.ToInt32(qty);

                var userId = context.UserData.Get<int>(StateKeys.UserId);

                var db = new DrugBotDataContext();
                var user = db.Users.FirstOrDefault(x => x.UserId == userId);

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

                    db.Commit();

                    await context.PostAsync($"You sold {qty} for {total:C0}.");
                    await context.PostAsync($"You have {user.Wallet:C0} in your wallet.");
                    context.Done<object>(null);
                }
                else
                {
                    await context.PostAsync("You don't have that much to sell...");
                    context.Done<object>(null);
                }
            }
        }
    }
}