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
    public class BuyGunDialog : BaseDialog, IDialog<GameState>
    {
        public async Task StartAsync(IDialogContext context)
        {
            var user = this.GetUser(context);
            var gun = this.GetRandomGun(user.DayOfGame);
            context.UserData.SetValue(StateKeys.GunToBuy, gun);

            PromptDialog.Confirm(
                context,
                ConfirmPurchase,
                $"Psst--you wanna buy this {gun.Name} for {gun.Cost:C0}? It does {gun.Damage} damage!" +
                    $"{(user.GunId.HasValue && user.GunId.Value > 0 ? " It'll replace that other thing you're carryin'" : string.Empty)}",
                "The #$^! did you just say?! Gimmie a yes or no, ya punk...",
                promptStyle: PromptStyle.None);
        }

        private async Task ConfirmPurchase(IDialogContext context, IAwaitable<bool> result)
        {
            var confirm = await result;

            if (confirm)
            {
                var buyGunResult = this.BuyGun(context);
                if (buyGunResult.IsSuccessful)
                {
                    await context.PostAsync("Pleasure doin' business. Be careful wit' that thing.");
                }
                else
                {
                    await context.PostAsync($"{buyGunResult.ReasonText} Later!");
                }
            }
            else
            {
                await context.PostAsync("Too bad, kid...");
            }

            context.UserData.RemoveValue(StateKeys.GunToBuy);

            this.Done(context);
        }
    }
}