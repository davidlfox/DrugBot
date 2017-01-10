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
    public class CombatDialog : BaseDialog, IDialog<GameState>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await ShowMainCombatMenu(context);

            context.Wait(MessageReceivedAsync);
        }

        private async Task ShowMainCombatMenu(IDialogContext context)
        {
            var user = this.GetUser(context);

            var actions = new List<string>();
            actions.Add("Melee");

            // does have a gun?
            if (user.GunId > 0)
            {
                actions.Add("Shoot");
            }

            actions.Add("Run Away");

            // display a menu of options
            var buttons = this.CreateButtonMenu(actions);
            IMessageActivity activity = this.SetupActivity(context, buttons,
                $"$#^! it's the cops! One of 'em and he's a big boy! What are you going to do?");

            await context.PostAsync(activity);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            var combat = this.GetCombatContext(context);

            switch (message.Text.ToLower())
            {
                case "melee":
                case "punch":
                case "kick":
                    // do some random weak damage
                    var damage = RandomEvent.GetRandomNumberBetween(5, 11);
                    await DoFighting(context, combat, damage);
                    break;
                case "shoot":
                case "pew":
                    // check for user gun
                    var user = this.GetUser(context);
                    if (user.GunId > 0)
                    {
                        await DoFighting(context, combat, user.Gun.Damage);
                    }
                    else
                    {
                        await context.PostAsync("You don't exactly have a gun...");
                        await this.ShowMainCombatMenu(context);
                        context.Wait(MessageReceivedAsync);
                    }
                    break;
                case "run":
                case "run away":
                    // roll the dice
                    var couldRunAway = RandomEvent.GetRandomNumberBetween(0, 2) == 1;
                    if (couldRunAway)
                    {
                        await context.PostAsync("Got lucky--you managed to outrun them!");
                        this.Done(context);
                    }
                    else
                    {
                        var returnDamage = RandomEvent.GetRandomNumberBetween(5, 20);
                        combat.HitPlayer(returnDamage);
                        if (combat.PlayerHitPoints > 0)
                        {
                            await context.PostAsync($"You couldn't get away--they hit you for {returnDamage}! You have {combat.PlayerHitPoints} life left.");
                            context.UserData.SetValue(StateKeys.CombatContext, combat);
                            await this.ShowMainCombatMenu(context);
                            context.Wait(MessageReceivedAsync);
                        }
                        else
                        {
                            await PlayerDead(context);
                        }
                    }
                    break;
                default:
                    await context.PostAsync("I didn't understand that...you should probably type MELEE, SHOOT, or RUN.");
                    break;
            }
        }

        private async Task DoFighting(IDialogContext context, CombatContext combat, int damage)
        {
            combat.HitEnemy(damage);
            if (combat.EnemyHitPoints > 0)
            {
                // todo: make configurable
                var returnDamage = RandomEvent.GetRandomNumberBetween(7, 22);
                combat.HitPlayer(returnDamage);
                await context.PostAsync($"You hit 'em for {damage} points of damage--they have {combat.EnemyHitPoints} life left!\n\n" +
                    $"They hit you back for {returnDamage} points! You have {combat.PlayerHitPoints} life left.");

                context.UserData.SetValue(StateKeys.CombatContext, combat);

                if (combat.PlayerHitPoints > 0)
                {
                    await this.ShowMainCombatMenu(context);
                    context.Wait(MessageReceivedAsync);
                }
                else
                {
                    await PlayerDead(context);
                }
            }
            else
            {
                await context.PostAsync("You managed to fight them off!");
                this.Done(context);
            }
        }

        private async Task PlayerDead(IDialogContext context)
        {
            context.UserData.RemoveValue(StateKeys.CombatContext);

            var lostInventory = string.Empty;
            var lostWallet = 0;
            InventoryItem stolenDrug = null;
            var stolenQuantity = 0;

            var db = new DrugBotDataContext();
            var userId = context.UserData.Get<int>(StateKeys.UserId);
            var user = db.Users.Single(x => x.UserId == userId);

            // randomize some losses
            if (user.Wallet > 0)
            {
                var lossRate = RandomEvent.GetRandomDoubleBetween(0.05, 0.10);
                lostWallet = (int)(user.Wallet * lossRate);
                user.Wallet = user.Wallet - lostWallet;
            }

            if (user.Inventory.Any(x => x.Quantity > 0))
            {
                var availableInventory = db.InventoryItems
                    .Where(x => x.UserId == userId && x.Quantity > 0);
                var count = availableInventory.Count();
                var drugIndex = RandomEvent.GetRandomNumberBetween(0, count);

                stolenDrug = availableInventory.ToList().ElementAt(drugIndex);
                var stealRate = RandomEvent.GetRandomDoubleBetween(0.05, 0.10);
                stolenQuantity = (int)(stolenDrug.Quantity * stealRate);
                stolenDrug.Quantity = stolenDrug.Quantity - stolenQuantity;
            }

            db.Commit();

            lostInventory = $"{(lostWallet > 0 ? $"{lostWallet:C0} from your wallet" : string.Empty)}" +
                $"{(lostWallet > 0 && stolenQuantity > 0 ? " and " : string.Empty)}" +
                $"{(stolenQuantity > 0 ? $"{stolenQuantity} {stolenDrug.Drug.Name} from you" : string.Empty)}";

            await context.PostAsync($"They beat you unconscious and took {lostInventory}.");
            this.Done(context);
        }
    }
}