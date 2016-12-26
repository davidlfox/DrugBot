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
    public class GameDialog : BaseDialog, IDialog<GameState>
    {
        string BotUserId;
        string Name;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (message.Text.ToLower() == "play")
            {
                BotUserId = message.Conversation.Id;

                var db = new DrugBotDataContext();
                var user = db.FindUser(message.Conversation.Id);

                // start in washington, dc
                context.UserData.SetValue<int>(StateKeys.LocationId, 1);
                db.Commit();

                if (user == null)
                {
                    // first time playing, create user, prompt for name...
                    PromptDialog.Text(context, SetupNameAsync, "What's your name?", "retry...");
                }
                else
                {
                    // todo: greet user
                    await context.PostAsync("I know you...");
                    context.UserData.SetValue<int>(StateKeys.UserId, user.UserId);
                    context.Call(new MainMenuDialog(), BackToSetupNameAsync);
                }
            }
            else
            {
                // tell them to trigger the game start
                await context.PostAsync("You should probably type \"PLAY\" to start the game...");
                context.Wait(MessageReceivedAsync);
            }
        }

        public virtual async Task SetupNameAsync(IDialogContext context, IAwaitable<string> result)
        {
            Name = await result;

            var db = new DrugBotDataContext();
            var user = db.AddUser(BotUserId, Name);
            db.Commit();

            context.UserData.SetValue<int>(StateKeys.UserId, user.UserId);

            await context.PostAsync($"Thanks {Name}...let's make some money!");

            // start in washington, dc
            context.UserData.SetValue<int>(StateKeys.LocationId, 1);

            // go to main menu
            context.Call(new MainMenuDialog(), BackToSetupNameAsync);
        }

        // callback from setup name soooo, as we're popping this off stack
        private async Task BackToSetupNameAsync(IDialogContext context, IAwaitable<GameState> result)
        {
            var message = await result;
            context.Done(message);
        }
    }
}