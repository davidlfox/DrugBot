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
    public class TravelDialog : BaseDialog, IDialog<GameState>
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

            var locationId = context.UserData.Get<int>(StateKeys.LocationId);
            var location = locations.Single(x => x.LocationId == locationId);

            await context.PostAsync(this.SetupHeroResponse(context, buttons, $"You're in {location.Name}. Where do you wanna go? It's gonna cost you a day!"));

            // get day of game
            var user = this.GetUser(context);
            if (user.DayOfGame == Defaults.GameEndDay - 1)
            {
                await context.PostAsync(Defaults.GameEndWarningText);
            }

            context.Wait(MessageReceivedAsync);
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
                var locations = this.GetLocationsWithLower();
                // try to figure out where they want to go
                var dst = locations.FirstOrDefault(x => x.NameLower == message.Text.ToLower() 
                    || x.NameLower.StartsWith(message.Text.ToLower()));

                if (dst == null)
                {
                    await context.PostAsync("I don't know where that is...");
                    this.Done(context);
                }
                else
                {
                    // check to make sure they're actually leaving
                    var locationId = context.UserData.Get<int>(StateKeys.LocationId);

                    if (locationId == dst.LocationId)
                    {
                        await context.PostAsync("You're already here...");
                        this.Done(context);
                    }
                    else
                    {
                        var day = this.TravelUser(context.UserData.Get<int>(StateKeys.UserId), dst.LocationId);

                        context.UserData.SetValue<int>(StateKeys.LocationId, dst.LocationId);

                        if (day == Defaults.GameEndDay)
                        {
                            // end game
                            this.Done(context, new GameState { IsGameOver = true });
                        }
                        else
                        {
                            this.GetDrugPrices(context, true);

                            await context.PostAsync($"Off to {dst.Name}!");
                            this.Done(context, new GameState { IsTraveling = true });
                        }
                    }
                }
            }
        }
    }
}