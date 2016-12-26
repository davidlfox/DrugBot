using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using DrugBot.Common;

namespace DrugBot.Dialogs
{
    [Serializable]
    public class MainMenuDialog : BaseDialog, IDialog<GameState>
    {
        /// <summary>
        /// Print menu to navigate between traveling, buying, selling, etc
        /// </summary>
        public async Task StartAsync(IDialogContext context)
        {
            // setup hero card
            var buttons = new List<CardAction>();
            buttons.Add(new CardAction
            {
                Title = "Travel",
                Type = ActionTypes.ImBack,
                Value = "Travel",
            });
            buttons.Add(new CardAction
            {
                Title = "Buy",
                Type = ActionTypes.ImBack,
                Value = "Buy",
            });
            buttons.Add(new CardAction
            {
                Title = "Sell",
                Type = ActionTypes.ImBack,
                Value = "Sell",
            });

            var user = this.GetUser(context);

            HeroCard heroCard = new HeroCard
            {
                Buttons = buttons,
                Text = $"What do you want to do? You have {user.Wallet:C0}."
            };

            var attachment = heroCard.ToAttachment();

            // setup reply
            var activity = context.MakeMessage();
            activity.Attachments = new List<Attachment>();
            activity.Attachments.Add(attachment);

            await context.PostAsync(activity);
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            switch (message.Text.ToLower())
            {
                case "travel":
                    context.Call(new TravelDialog(), ResumeMainMenu);
                    break;
                case "buy":
                    context.Call(new BuyDialog(), ResumeMainMenu);
                    break;
                case "sell":
                    context.Call(new SellDialog(), ResumeMainMenu);
                    break;
                case "prices":
                    // todo: display current prices
                    throw new NotImplementedException();
                    break;
                default:
                    await context.PostAsync("I don't understand that. You should probably type TRAVEL, or BUY, or SELL to continue...");
                    context.Wait(MessageReceivedAsync);
                    break;
            }
        }

        private async Task ResumeMainMenu(IDialogContext context, IAwaitable<GameState> result)
        {
            var state = await result;

            if (state.IsGameOver)
            {
                var money = this.ResetUser(context);
                await context.PostAsync("Game Over.");
                await context.PostAsync($"You finished with {money:C0}.");
                this.Done(context);
            }
            else if (state.IsTraveling)
            {
                // get day and location
                var user = this.GetUser(context);
                var location = this.GetLocations().Single(x => x.LocationId == user.LocationId);
                await context.PostAsync($"It's day {user.DayOfGame}. You're in {location.Name}.");
                await StartAsync(context);
            }
            else
            {
                // print menu and start again--hopefully
                await StartAsync(context);
            }
        }
    }
}