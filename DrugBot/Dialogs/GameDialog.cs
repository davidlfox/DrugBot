using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using DrugBot.Data;
using DrugBot.Common;
using System.Web.ModelBinding;

namespace DrugBot.Dialogs
{
    [Serializable]
    public class GameDialog : BaseDialog, IDialog<GameState>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            this.BotUserId = message.Conversation.Id;

            if (message.Text.ToLower() == "play")
            {

                var db = new DrugBotDataContext();
                var user = db.FindUser(this.BotUserId);

                // start in washington, dc
                context.UserData.SetValue<int>(StateKeys.LocationId, 1);
                db.Commit();

                if (user == null)
                {
                    // first time playing, create user, prompt for name...
                    context.Call(new SetupNameDialog(), BackToSetupNameAsync);
                }
                else
                {
                    // todo: greet user
                    await context.PostAsync("I know you...");
                    context.UserData.SetValue<int>(StateKeys.UserId, user.UserId);
                    context.Call(new MainMenuDialog(), BackToSetupNameAsync);
                }
            }
            else if (message.Text.ToLower() == "leaderboard")
            {
                await this.ShowLeaderboard(context);
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                // tell them to trigger the game start
                await context.PostAsync("You should probably type PLAY to start the game...");
                context.Wait(MessageReceivedAsync);
            }
        }

        // callback from setup name soooo, as we're popping this off stack
        private async Task BackToSetupNameAsync(IDialogContext context, IAwaitable<GameState> result)
        {
            var message = await result;
            if (message.IsNameReady)
            {
                context.Call(new MainMenuDialog(), BackToSetupNameAsync);
            }
            else
            {
                context.Done(message);
            }
        }
    }
}