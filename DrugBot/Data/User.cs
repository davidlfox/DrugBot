using DrugBot.Common;
using DrugBot.Data.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace DrugBot.Data
{
    public class User
    {
        public User()
        {
            this.InventorySize = Defaults.InventorySize;
        }

        /// <summary>
        /// Primary key, not necessarily the bot framework's definition of user id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The unique conversation id identifying the user. I think this is cool while the app is only setup for facebook
        /// </summary>
        [Required]
        public string BotUserId { get; set; }

        /// <summary>
        /// Display name
        /// </summary>
        [StringLength(30, ErrorMessage = "Too long--try using fewer than 30 characters")]
        [Required]
        //[UniqueUserName(ErrorMessage = "Name must be unique")]
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
        /// Max inventory size
        /// </summary>
        public int InventorySize { get; set; }

        /// <summary>
        /// drugs on hand
        /// </summary>
        public virtual ICollection<InventoryItem> Inventory { get; set; }
    }
}