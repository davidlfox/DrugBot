using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DrugBot.Common
{
    public class GameState
    {
        public bool IsNameReady { get; set; }
        public bool IsGameOver { get; set; }
        public bool IsTraveling { get; set; }
    }
}