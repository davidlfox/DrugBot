using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DrugBot.Common
{
    public class Defaults
    {
        public readonly static int StartingMoney = 1000;
        public readonly static int GameStartDay = 1;
        public readonly static int LocationId = 1;
        public readonly static int GameEndDay = 3;
        public readonly static string GameEndWarningText = "WARNING: This is the last day. You should offload your stash now. Your score is based on what's in your wallet--not inventory!";
        public readonly static byte LeaderboardSize = 5;
        public readonly static int InventorySize = 1000;
        public readonly static int MaxLoanMultiplier = 10000;
    }
}