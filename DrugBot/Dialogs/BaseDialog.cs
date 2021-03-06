﻿using DrugBot.Common;
using DrugBot.Data;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DrugBot.Dialogs
{
    [Serializable]
    public class BaseDialog
    {
        protected string BotUserId;
        protected string Name;

        private static readonly Random rand = new Random();

        protected void Done(IDialogContext context)
        {
            context.Done(new GameState { });
        }

        protected void Done(IDialogContext context, GameState state)
        {
            context.Done(state);
        }

        protected IMessageActivity SetupHeroResponse(IDialogContext context, List<CardAction> buttons, string text)
        {
            // setup hero card
            HeroCard heroCard = new HeroCard
            {
                Buttons = buttons,
                Text = text,
            };

            // setup attachment
            var attachment = heroCard.ToAttachment();

            // send hero card to user
            var activity = context.MakeMessage();
            activity.Attachments = new List<Attachment>();
            activity.Attachments.Add(attachment);

            return activity;
        }

        protected User GetUser(IDialogContext context)
        {
            var userId = context.UserData.Get<int>(StateKeys.UserId);
            var db = new DrugBotDataContext();
            return db.Users.FirstOrDefault(x => x.UserId == userId);
        }

        protected Dictionary<int, int> GetDrugPrices(IDialogContext context, bool newPrices = false)
        {
            var drugPrices = new Dictionary<int, int>();

            var db = new DrugBotDataContext();
            var drugs = db.GetDrugs().ToList();

            if (!context.UserData.TryGetValue(StateKeys.DrugPrices, out drugPrices) || newPrices)
            {
                drugPrices = new Dictionary<int, int>();

                foreach (var drug in drugs)
                {
                    var price = rand.Next(drug.MinPrice, drug.MaxPrice);
                    drugPrices.Add(drug.DrugId, price);
                }

                // store to state
                context.UserData.SetValue(StateKeys.DrugPrices, drugPrices);
            }

            return drugPrices;
        }

        protected IQueryable<Location> GetLocations()
        {
            var db = new DrugBotDataContext();
            return db.Locations;
        }

        protected List<LocationWithLower> GetLocationsWithLower()
        {
            return this.GetLocations()
                .ToList()
                .Select(x => new LocationWithLower
                {
                    LocationId = x.LocationId,
                    Name = x.Name,
                    NameLower = x.Name.ToLower(),
                })
                .ToList();
        }

        protected Location GetLocation(IDialogContext context)
        {
            var locationId = context.UserData.Get<int>(StateKeys.LocationId);
            var db = new DrugBotDataContext();
            return db.Locations.Single(x => x.LocationId == locationId);
        }

        protected int TravelUser(int userId, int locationId)
        {
            var db = new DrugBotDataContext();
            var user = db.Users.FirstOrDefault(x => x.UserId == userId);
            if (user != null)
            {
                user.LocationId = locationId;
                user.DayOfGame = user.DayOfGame + 1;
                if (user.LoanDebt > 0)
                {
                    user.LoanDebt = user.LoanDebt + (int)(user.LoanDebt * user.LoanRate);
                }

                db.Commit();

                return user.DayOfGame;
            }

            return 0;
        }

        protected void ResetUser(IDialogContext context)
        {
            var db = new DrugBotDataContext();
            var userId = context.UserData.Get<int>(StateKeys.UserId);
            var user = db.Users.FirstOrDefault(x => x.UserId == userId);

            var money = user.Wallet;

            user.Wallet = Defaults.StartingMoney;
            user.DayOfGame = Defaults.GameStartDay;
            user.LocationId = Defaults.LocationId;
            user.LoanDebt = 0;
            user.LoanRate = 0.0;
            user.GunId = null;
            user.InventorySize = Defaults.InventorySize;

            // clear out inventory
            db.InventoryItems.RemoveRange(user.Inventory);

            // regenerate drug prices for this user
            this.GetDrugPrices(context, true);

            db.Commit();
        }

        public List<CardAction> GetDrugButtons(IDialogContext context)
        {
            var drugPrices = this.GetDrugPrices(context);
            var drugs = this.GetDrugs();
            var buttons = new List<CardAction>();

            foreach (var drugPrice in drugPrices)
            {
                var drug = drugs.Single(x => x.DrugId == drugPrice.Key);

                buttons.Add(new CardAction
                {
                    Title = $"{drug.Name}: {drugPrice.Value:C0}",
                    Type = ActionTypes.ImBack,
                    Value = drug.Name,
                });
            }

            return buttons;
        }

        protected IQueryable<Drug> GetDrugs()
        {
            var db = new DrugBotDataContext();
            return db.Drugs;
        }

        protected async Task ShowInventory(IDialogContext context)
        {
            var user = this.GetUser(context);
            var usedSpace = user.Inventory.Sum(x => x.Quantity);

            var sb = new StringBuilder($"Inventory: ({usedSpace}/{user.InventorySize})\n\n");

            if (user.Inventory.Any(x => x.Quantity > 0) || user.GunId.HasValue)
            {
                foreach (var inventory in user.Inventory)
                {
                    // could have quantity: 0 in some cases, dont bother displaying
                    if (inventory.Quantity > 0)
                    {
                        sb.Append($"{inventory.Drug.Name}: {inventory.Quantity}\n\n");
                    }
                }

                if (user.GunId.HasValue)
                {
                    sb.Append($"{user.Gun.Name} ({user.Gun.Damage} damage)\n\n");
                }
            }
            else
            {
                sb.Append("Your inventory is empty");
            }

            await context.PostAsync(sb.ToString());
        }

        protected async Task ShowPrices(IDialogContext context)
        {
            var drugPrices = this.GetDrugPrices(context);
            var drugs = this.GetDrugs().ToList();

            var sb = new StringBuilder("Prices:\n\n");

            foreach(var drugPrice in drugPrices)
            {
                var drug = drugs.Single(x => x.DrugId == drugPrice.Key);
                sb.Append($"{drug.Name}: {drugPrice.Value:C0}\n\n");
            }

            await context.PostAsync(sb.ToString());
        }

        protected async Task ShowLeaderboard(IDialogContext context)
        {
            var db = new DrugBotDataContext();

            // show leaderboard
            var leaders = db.GetLeaderboard();

            var sb = new StringBuilder("Leaderboard:\n\n");

            foreach (var leader in leaders)
            {
                sb.Append($"{leader.User.Name}: {leader.Score:C0}\n\n");
            }

            await context.PostAsync(sb.ToString());
        }

        protected void SetupLoan(IDialogContext context, int amount, double rate)
        {
            var db = new DrugBotDataContext();
            var userId = context.UserData.Get<int>(StateKeys.UserId);
            var user = db.Users.FirstOrDefault(x => x.UserId == userId);

            user.LoanDebt = amount;
            user.LoanRate = rate;
            user.Wallet = user.Wallet + amount;

            db.Commit();
        }

        protected bool PayLoan(IDialogContext context)
        {
            var db = new DrugBotDataContext();
            var userId = context.UserData.Get<int>(StateKeys.UserId);
            var user = db.Users.FirstOrDefault(x => x.UserId == userId);

            if (user.Wallet >= user.LoanDebt)
            {
                user.Wallet = user.Wallet - user.LoanDebt;
                user.LoanDebt = 0;
                user.LoanRate = 0.0;
                db.Commit();

                context.UserData.SetValue<double>(StateKeys.LoanRate, 0.0);

                return true;
            }

            return false;
        }

        protected Gun GetRandomGun(int dayOfGame)
        {
            var db = new DrugBotDataContext();
            var guns = db.Guns.Where(x => x.MinimumDayOfGame <= dayOfGame).ToList();
            return guns.ElementAt(RandomEvent.GetRandomNumberBetween(0, guns.Count));
        }

        protected BuyGunInfo BuyGun(IDialogContext context)
        {
            var db = new DrugBotDataContext();
            var gun = context.UserData.Get<Gun>(StateKeys.GunToBuy);
            var userId = context.UserData.Get<int>(StateKeys.UserId);
            var user = db.Users.FirstOrDefault(x => x.UserId == userId);

            if (user.Wallet >= gun.Cost)
            {
                user.Wallet = user.Wallet - gun.Cost;
                user.GunId = gun.GunId;
                db.Commit();

                return new BuyGunInfo { IsSuccessful = true };
            }

            return new BuyGunInfo { IsSuccessful = false, ReasonText = "You ain't got the cash for this piece." };
        }

        protected CombatContext GetCombatContext(IDialogContext context)
        {
            return context.UserData.Get<CombatContext>(StateKeys.CombatContext);
        }

        protected IMessageActivity SetupActivity(IDialogContext context, List<CardAction> buttons, string menuText)
        {
            HeroCard heroCard = new HeroCard
            {
                Buttons = buttons,
                Text = menuText,
            };

            var attachment = heroCard.ToAttachment();

            // setup reply
            var activity = context.MakeMessage();
            activity.Attachments = new List<Attachment>();
            activity.Attachments.Add(attachment);
            return activity;
        }

        protected List<CardAction> CreateButtonMenu(IEnumerable<string> buttons)
        {
            var result = new List<CardAction>();

            foreach (var action in buttons)
            {
                result.Add(new CardAction { Title = action, Type = ActionTypes.ImBack, Value = action });
            }

            return result;
        }

        protected void AddCancelButton(ICollection<CardAction> buttons)
        {
            buttons.Add(new CardAction
            {
                Title = "Cancel",
                Type = ActionTypes.ImBack,
                Value = "Cancel",
            });
        }
    }

    public class LocationWithLower
    {
        public int LocationId { get; set; }
        public string Name { get; set; }
        public string NameLower { get; set; }
    }
}