using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace DrugBot.Data
{
    public class DrugBotDataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Drug> Drugs { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Location> Locations { get; set; }
    }

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

        [ForeignKey("Location")]
        public int? LocationId { get; set; }
        public virtual Location Location { get; set; }

        /// <summary>
        /// drugs on hand
        /// </summary>
        public virtual ICollection<InventoryItem> Inventory { get; set; }
    }

    public class Drug
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int DrugId { get; set; }

        /// <summary>
        /// Drug name e.g. "Weed"
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Guess we'll use this at some point
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Minimum randomly generated price
        /// </summary>
        public int MinPrice { get; set; }

        /// <summary>
        /// Maximum randomly generated price
        /// </summary>
        public int MaxPrice { get; set; }

    }

    public class Game
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// This may be BF's conversation id
        /// </summary>
        public string ConversationId { get; set; }

        /// <summary>
        /// Day of game cycle
        /// </summary>
        public byte GameDay { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }

    public class Location
    {
        public int LocationId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class InventoryItem
    {
        public int InventoryItemId { get; set; }

        [ForeignKey("Drug")]
        public int DrugId { get; set; }
        public virtual Drug Drug { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }

    }
}