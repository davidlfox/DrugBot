using DrugBot.Common;
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

                var rand = new Random();

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

            var sb = new StringBuilder("Inventory:\n\n");

            if(user.Inventory.Any(x => x.Quantity > 0))
            {
                foreach (var inventory in user.Inventory)
                {
                    // could have quantity: 0 in some cases, dont bother displaying
                    if(inventory.Quantity > 0)
                    {
                        sb.Append($"{inventory.Drug.Name}: {inventory.Quantity}\n\n");
                    }
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

        protected async Task ShowLeaderboard(IDialogContext context, DrugBotDataContext db)
        {
            // show leaderboard
            var leaders = db.GetLeaderboard();

            var sb = new StringBuilder("Leaderboard:\n\n");

            foreach (var leader in leaders)
            {
                sb.Append($"{leader.User.Name}: {leader.Score:C0}\n\n");
            }

            await context.PostAsync(sb.ToString());
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