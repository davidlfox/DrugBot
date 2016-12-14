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
    }

    public class User
    {
        public int UserId { get; set; }
        /// <summary>
        /// not sure if we need this
        /// </summary>
        public string BotUserId { get; set; }
        public string Name { get; set; }
        public decimal Wallet { get; set; }
    }

    public class Drug
    {
        public int DrugId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class Game
    {
        public int GameId { get; set; }
        public string ConversationId { get; set; }
        public byte GameDay { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}