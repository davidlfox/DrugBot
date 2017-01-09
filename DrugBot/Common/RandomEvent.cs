using DrugBot.Data;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DrugBot.Common
{
    public class RandomEvent
    {
        public static readonly double OddsOfRandomEvent = 0.5;

        private static readonly Random rand = new Random();

        public static bool IsGoingToHappen
        {
            get
            {
                return rand.NextDouble() < OddsOfRandomEvent;
            }
        }

        public static EventInfo Get(IDialogContext context, int userId)
        {
            var eventText = string.Empty;

            // random weighted event
            var eventOdd = rand.Next(1, 6);

            System.Diagnostics.Debug.WriteLine($"eventOdd: {eventOdd}");

            // todo: some kind of weighted dictionary/enum/whatevs to handle this randomization

            var db = new DrugBotDataContext();

            switch (eventOdd)
            {
                // event: mugging
                case 1:
                    eventText = DoMugging(userId, db);
                    break;
                // event: police raid
                case 2:
                    eventText = DoDrugSpike(userId, db, context);
                    break;
                // event: drug spike down
                case 3:
                    eventText = DoDrugSpikeDown(userId, db, context);
                    break;
                // event: found a trenchcoat
                case 4:
                    eventText = DoTrenchcoat(userId, db);
                    break;
                // event: option to buy a gun, handled later
                case 5:
                    return new EventInfo { IsGunEvent = true };
                    break;
            }

            return new EventInfo { EventText = eventText };
        }

        private static string DoMugging(int userId, DrugBotDataContext db)
        {
            var user = db.Users.Single(x => x.UserId == userId);

            // get current wallet
            var wallet = user.Wallet;

            // todo: put bounds somewhere static or configurable
            var lossRate = GetRandomDoubleBetween(0.10, 0.30);
            var moneyLost = (int)(wallet * lossRate);
            var muggingText = RandomMuggingText();

            user.Wallet = user.Wallet - moneyLost;
            db.Commit();

            return $"{muggingText} You lost {moneyLost:C0}!";
        }

        private static string DoDrugSpike(int userId, DrugBotDataContext db, IDialogContext context)
        {
            // randomize the drug to spike
            var drugs = db.GetDrugs().ToList();
            var drug = drugs.ElementAt(rand.Next(0, drugs.Count()));

            // todo: put these bounds somewhere static or configurable
            // adjust drug price, 50% to 400% over the max
            var spikeRate = GetRandomDoubleBetween(0.50, 4.0);
            var maxPrice = drug.MaxPrice;
            var spikePrice = (int)(maxPrice + (maxPrice * spikeRate));

            var drugPrices = context.UserData.Get<Dictionary<int, int>>(StateKeys.DrugPrices);
            drugPrices[drug.DrugId] = spikePrice;
            context.UserData.SetValue(StateKeys.DrugPrices, drugPrices);

            var spikeText = RandomSpikeText();
            return $"{spikeText} {drug.Name} prices have gone through the roof!";
        }

        private static string DoDrugSpikeDown(int userId, DrugBotDataContext db, IDialogContext context)
        {
            // randomize the drug to spike
            var drugs = db.GetDrugs().ToList();
            var drug = drugs.ElementAt(rand.Next(0, drugs.Count()));

            // todo: put these bounds somewhere static or configurable
            // adjust drug price, % of the min
            var spikeRate = GetRandomDoubleBetween(0.20, 0.40);
            var minPrice = drug.MinPrice;
            var spikePrice = (int)(minPrice * spikeRate);
            if (spikePrice <= 0)
            {
                spikePrice = 1;
            }

            var drugPrices = context.UserData.Get<Dictionary<int, int>>(StateKeys.DrugPrices);
            drugPrices[drug.DrugId] = spikePrice;
            context.UserData.SetValue(StateKeys.DrugPrices, drugPrices);

            var spikeText = RandomDownSpikeText();
            return $"{spikeText} {drug.Name} prices have bottomed out!";
        }

        private static string DoTrenchcoat(int userId, DrugBotDataContext db)
        {
            var user = db.Users.Single(x => x.UserId == userId);

            // get storage
            var currentSpace = user.InventorySize;

            // get increase rate
            // todo: make static/configurable
            var newSpaceRate = GetRandomDoubleBetween(0.30, 0.50);
            var newSpace = (int)(currentSpace * newSpaceRate);

            // commit to user
            user.InventorySize = user.InventorySize + newSpace;
            db.Commit();

            var trenchcoatText = RandomTrenchcoatText();
            return $"{trenchcoatText} You can hold {newSpace} more drugs!";
        }

        public static int GetRandomNumberBetween(int min, int max)
        {
            return rand.Next(min, max);
        }

        public static double GetRandomDoubleBetween(double min, double max)
        {
            return rand.NextDouble() * (max - min) + min;
        }

        public static string RandomMuggingText()
        {
            return "You got mugged!";
        }

        public static string RandomSpikeText()
        {
            return "Police raid!";
        }

        public static string RandomDownSpikeText()
        {
            return "Junkies are dying everywhere!";
        }

        public static string RandomTrenchcoatText()
        {
            return "You found a bigger trenchcoat!";
        }
    }
}