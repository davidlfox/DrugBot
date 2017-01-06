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
        public static readonly double OddsOfRandomEvent = 0.50;

        public static bool IsGoingToHappen
        {
            get
            {
                var rand = new Random();
                return rand.NextDouble() < OddsOfRandomEvent;
            }
        }

        public static string Get(IDialogContext context, int userId)
        {
            var eventText = string.Empty;

            // random weighted event
            var rand = new Random();
            var eventOdd = rand.NextDouble();

            // todo: some kind of weighted dictionary/enum/whatevs to handle this randomization

            var db = new DrugBotDataContext();

            // event: mugging
            if(eventOdd < 0.25)
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

                eventText = $"{muggingText} You lost {moneyLost:C0}!";
            }
            // event: police raid
            else if (eventOdd < 0.50)
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
                eventText = $"{spikeText} {drug.Name} prices have gone through the roof!";
            }
            // event: drug spike down
            else if (eventOdd < 0.75)
            {
                // randomize the drug to spike
                var drugs = db.GetDrugs().ToList();
                var drug = drugs.ElementAt(rand.Next(0, drugs.Count()));

                // todo: put these bounds somewhere static or configurable
                // adjust drug price, % of the min
                var spikeRate = GetRandomDoubleBetween(0.20, 0.40);
                var minPrice = drug.MinPrice;
                var spikePrice = (int)(minPrice * spikeRate);
                if(spikePrice <= 0)
                {
                    spikePrice = 1;
                }

                var drugPrices = context.UserData.Get<Dictionary<int, int>>(StateKeys.DrugPrices);
                drugPrices[drug.DrugId] = spikePrice;
                context.UserData.SetValue(StateKeys.DrugPrices, drugPrices);

                var spikeText = RandomDownSpikeText();
                eventText = $"{spikeText} {drug.Name} prices have bottomed out!";
            }
            // event: found a trenchcoat
            else
            {
                var user = db.Users.Single(x => x.UserId == userId);

                // get storage
                var currentSpace = user.InventorySize;

                // get increase rate
                // todo: make static/configurable
                var newSpaceRate = GetRandomDoubleBetween(0.30, 0.60);
                var newSpace = (int)(currentSpace * newSpaceRate);

                // commit to user
                user.InventorySize = user.InventorySize + newSpace;
                db.Commit();

                var trenchcoatText = RandomTrenchcoatText();
                eventText = $"{trenchcoatText} You can hold {newSpace} more drugs!";
            }

            return eventText;
        }

        public static double GetRandomDoubleBetween(double min, double max)
        {
            var rand = new Random();
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