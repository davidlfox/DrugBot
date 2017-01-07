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
    public class LoanDialog : BaseDialog, IDialog<GameState>
    {
        public async Task StartAsync(IDialogContext context)
        {
            var user = this.GetUser(context);
            // force payback if loan outstanding
            if (user.LoanDebt > 0)
            {
                PromptDialog.Confirm(
                    context,
                    ReadyToPayAsync,
                    $"You already got a loan wit' me fer {user.LoanDebt:C0}. Ready to pay up?",
                    "Didn't hear ya'--you ready to pay or what?",
                    promptStyle: PromptStyle.None);
            }
            else
            {
                // randomize rate between 5 and 15 percent
                double rate = 0.0;
                if (!context.UserData.TryGetValue<double>(StateKeys.LoanRate, out rate) || rate == 0.0)
                {
                    rate = RandomEvent.GetRandomDoubleBetween(0.05, 0.15);
                    context.UserData.SetValue<double>(StateKeys.LoanRate, rate);
                }

                var pointsRate = (int)(rate * 100);
                var maxLoan = user.DayOfGame * Defaults.MaxLoanMultiplier;

                // announce loan shark, explain rates
                await context.PostAsync("Here's how it's gonna work, kid. You only been here " +
                    $"{user.DayOfGame} day{(user.DayOfGame > 1 ? "s" : string.Empty)} I'll loan ya' up to {maxLoan:C0}, " +
                    $"but it's gonna cost ya' somethin' like {pointsRate} points a day. Capiche?");

                PromptDialog.Number(
                    context,
                    SetupLoanAsync,
                    "How much ya' want?",
                    "You stupid or somethin'? I need a number.");
            }
        }

        private async Task SetupLoanAsync(IDialogContext context, IAwaitable<long> result)
        {
            var amount = await result;
            if(amount > 0)
            {
                var user = this.GetUser(context);
                var maxLoan = user.DayOfGame * Defaults.MaxLoanMultiplier;
                if (amount > maxLoan)
                {
                    await context.PostAsync($"I told ya' already--I ain't trustin' ya' wit' more than {maxLoan:C0}!");
                    this.Done(context);
                }
                else
                {
                    var rate = context.UserData.Get<double>(StateKeys.LoanRate);
                    var pointsRate = (int)(rate * 100);
                    this.SetupLoan(context, (int)amount, rate);
                    await context.PostAsync($"Ok--get outta here wit' ya' money. Gonna cost ya' {pointsRate} a day, or I'll $^#! bury ya'");
                    this.Done(context);
                }
            }
            else
            {
                await context.PostAsync("I oughta smack the %^*! outta ya'");
                this.Done(context);
            }
        }

        private async Task ReadyToPayAsync(IDialogContext context, IAwaitable<bool> result)
        {
            var confirm = await result;

            if (confirm)
            {
                if (this.PayLoan(context))
                {
                    await context.PostAsync("Alright, kid--we're square.");
                }
                else
                {
                    await context.PostAsync("You ain't got the cash yet to pay me back--come back when you do.");
                }
            }
            else
            {
                await context.PostAsync("Then get the #$^! outta here!");
            }
            
            this.Done(context);
        }
    }
}