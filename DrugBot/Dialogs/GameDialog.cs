using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using DrugBot.Data;

namespace DrugBot.Dialogs
{
    [Serializable]
    public class GameDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (message.Text.ToLower() == "play")
            {
                await context.PostAsync("yeah, i'm published since deployment");
            }
            else
            {
                // mirror the text
                await context.PostAsync($"You said: {message.Text}. Say \"Hi\" to start the conversation.");
            }

            context.Wait(MessageReceivedAsync);
        }
    }
}