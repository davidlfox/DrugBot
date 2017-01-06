using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using DrugBot.Common;
using DrugBot.Data;
using System.Text;

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

            foreach (var action in new string[] { "Inventory", "Buy", "Sell", "Prices", "Travel" })
            {
                buttons.Add(new CardAction { Title = action, Type = ActionTypes.ImBack, Value = action });
            }

            var user = this.GetUser(context);

            var location = this.GetLocation(context);

            HeroCard heroCard = new HeroCard
            {
                Buttons = buttons,
                Text = $"You have {user.Wallet:C0}. You're in {location.Name}. What do you want to do?"
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
                case "inventory":
                    await this.ShowInventory(context);
                    // is this too quick to show them commands again?
                    await StartAsync(context);
                    break;
                case "prices":
                    await this.ShowPrices(context);
                    await StartAsync(context);
                    break;
                case "leaderboard":
                    await this.ShowLeaderboard(context);
                    await StartAsync(context);
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
                var user = this.GetUser(context);

                // record log of game before resetting
                var db = new DrugBotDataContext();
                db.AddGame(user.UserId, user.Wallet);
                db.Commit();

                await context.PostAsync("Game Over.");
                await context.PostAsync($"You finished with {user.Wallet:C0}.");
                await this.ShowLeaderboard(context);

                // reset user (this commits db changes)
                this.ResetUser(context);

                this.Done(context);
            }
            else if (state.IsTraveling)
            {
                var user = this.GetUser(context);

                // todo: random events for the day
                if (RandomEvent.IsGoingToHappen)
                {
                    // pass context and user to do db things, receive string
                    var eventText = RandomEvent.Get(context, user.UserId);
                    await context.PostAsync(eventText);
                }

                // get day and location
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