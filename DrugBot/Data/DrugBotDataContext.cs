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
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<Game> Games { get; set; }

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

        public void AddGame(int userId, long score)
        {
            this.Games.Add(new Game
            {
                Score = score,
                UserId = userId,
            });
        }

        // todo: get leaderboard collection
    }
}