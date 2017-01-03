using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace DrugBot.Data
{
    /// <summary>
    /// Log of a finished game
    /// </summary>
    public class Game
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// The player's final score
        /// </summary>
        public long Score { get; set; }

        /// <summary>
        /// Navigation property for 
        /// </summary>
        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}