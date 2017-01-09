using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DrugBot.Common
{
    public static class StateKeys
    {
        public readonly static string UserId = "userId";
        public readonly static string LocationId = "locationId";
        public readonly static string DrugPrices = "drugPrices";
        public readonly static string DrugToBuy = "drugToBuy";
        public readonly static string DrugToSell = "drugToSell";
        public readonly static string LoanRate = "loanRate";
        public readonly static string RandomEvent = "randomEvent";
        public readonly static string GunToBuy = "gunToBuy";
    }
}