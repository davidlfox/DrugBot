using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace DrugBot.Data
{
    public class User
    {
        /// <summary>
        /// Primary key, not necessarily the bot framework's definition of user id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// not sure if we need this
        /// </summary>
        public string BotUserId { get; set; }

        /// <summary>
        /// Display name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Money available
        /// </summary>
        public int Wallet { get; set; }

        /// <summary>
        /// Day of current game
        /// </summary>
        public int DayOfGame { get; set; }

        [ForeignKey("Location")]
        public int? LocationId { get; set; }
        public virtual Location Location { get; set; }

        /// <summary>
        /// drugs on hand
        /// </summary>
        public virtual ICollection<InventoryItem> Inventory { get; set; }
    }
}