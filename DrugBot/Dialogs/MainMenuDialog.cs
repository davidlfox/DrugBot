using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace DrugBot.Dialogs
{
    [Serializable]
    public class MainMenuDialog : IDialog<object>
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

            HeroCard heroCard = new HeroCard
            {
                Buttons = buttons,
                Text = "What do you want to do?"
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
                default:
                    await context.PostAsync("I don't understand that. You should probably type TRAVEL, or BUY, or SELL to continue...");
                    context.Wait(MessageReceivedAsync);
                    break;
            }
        }

        private async Task ResumeMainMenu(IDialogContext context, IAwaitable<object> result)
        {
            var message = await result;
            // print menu and start again--hopefully
            await StartAsync(context);
        }
    }
}