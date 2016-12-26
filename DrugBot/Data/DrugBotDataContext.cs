using DrugBot.Common;
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
        public DbSet<Location> Locations { get; set; }

        public int Commit()
        {
            return this.SaveChanges();
        }

        public User FindUser(string botUserId)
        {
            return this.Users.FirstOrDefault(x => x.BotUserId == botUserId);
        }

        public User AddUser(User user)
        {
            this.Users.Add(user);
            return user;
        }

        public User AddUser(string botUserID, string name)
        {
            var user = new User
            {
                BotUserId = botUserID,
                Name = name,
                Wallet = Defaults.StartingMoney,
                DayOfGame = Defaults.GameStartDay,
                LocationId = Defaults.LocationId,
            };

            this.AddUser(user);

            return user;
        }

        public Drug FindDrug(string name)
        {
            return this.Drugs.FirstOrDefault(x => x.Name == name);
        }

        public IQueryable<Drug> GetDrugs()
        {
            return this.Drugs;
        }
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

    public class Location
    {
        public int LocationId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class InventoryItem
    {
        public int InventoryItemId { get; set; }

        public int Quantity { get; set; }

        [ForeignKey("Drug")]
        public int DrugId { get; set; }
        public virtual Drug Drug { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }

    }
}