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
    public class TravelDialog : BaseDialog, IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            var locations = this.GetLocations();

            var buttons = new List<CardAction>();
            foreach(var loc in locations)
            {
                buttons.Add(new CardAction
                {
                    Title = loc.Name,
                    Type = ActionTypes.ImBack,
                    Value = loc.Name,
                });
            }

            this.AddCancelButton(buttons);

            await context.PostAsync(this.SetupHeroResponse(context, buttons, "Where do you wanna go? It's gonna cost you a day!"));
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (message.Text.ToLower() == "cancel")
            {
                context.Done<object>(null);
            }
        }
    }
}