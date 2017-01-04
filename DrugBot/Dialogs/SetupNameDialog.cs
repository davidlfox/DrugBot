using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using DrugBot.Common;
using DrugBot.Data;

namespace DrugBot.Dialogs
{
    [Serializable]
    public class SetupNameDialog : BaseDialog, IDialog<GameState>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("What is your name?");
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            var proposedName = message.Text;

            var db = new DrugBotDataContext();

            this.BotUserId = message.Conversation.Id;

            var validationError = db.ValidateUser(this.BotUserId, proposedName);

            // hack: check specifically for a unique name
            if(db.Users.Any(x => x.Name == proposedName))
            {
                validationError = "Your name must be unique...try again.";
            }

            // validate username, etc
            if (string.IsNullOrWhiteSpace(validationError))
            {
                var user = db.AddUser(this.BotUserId, proposedName);
                var records = db.Commit();

                this.Name = proposedName;

                context.UserData.SetValue<int>(StateKeys.UserId, user.UserId);

                await context.PostAsync($"Thanks {this.Name}...let's make some money!");

                // start in washington, dc
                context.UserData.SetValue<int>(StateKeys.LocationId, 1);

                // go back
                this.Done(context, new GameState { IsNameReady = true });
            }
            else
            {
                await context.PostAsync(validationError);

                // loop
                context.Wait(MessageReceivedAsync);
            }
        }
    }
}