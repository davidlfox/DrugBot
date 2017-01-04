using DrugBot.Common;
using DrugBot.Migrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
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

        public DrugBotDataContext()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<DrugBotDataContext, Configuration>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(30)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName, 
                    new IndexAnnotation(
                        new IndexAttribute("IX_UniqueName") { IsUnique = true }));

            // need this?
            base.OnModelCreating(modelBuilder);
        }

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

        public string ValidateUser(string botUserId, string name)
        {
            User user = GenerateNewUser(botUserId, name);

            var errors = new List<ValidationResult>();
            var context = new ValidationContext(user);
            if (!Validator.TryValidateObject(user, context, errors, true))
            {
                return errors.First().ErrorMessage;

            }

            return string.Empty;
        }

        public User AddUser(string botUserId, string name)
        {
            User user = GenerateNewUser(botUserId, name);

            this.AddUser(user);

            return user;
        }

        private static User GenerateNewUser(string botUserID, string name)
        {
            return new User
            {
                BotUserId = botUserID,
                Name = name,
                Wallet = Defaults.StartingMoney,
                DayOfGame = Defaults.GameStartDay,
                LocationId = Defaults.LocationId,
            };
        }

        public Drug FindDrug(string name)
        {
            return this.Drugs.FirstOrDefault(x => x.Name == name);
        }

        public IQueryable<Drug> GetDrugs()
        {
            return this.Drugs;
        }

        public void AddInventory(InventoryItem item)
        {
            this.InventoryItems.Add(item);
        }

        public void AddGame(int userId, long score)
        {
            this.Games.Add(new Game
            {
                Score = score,
                UserId = userId,
            });
        }

        public List<Game> GetLeaderboard()
        {
            return this.Games
                .OrderByDescending(x => x.Score)
                .Take(Defaults.LeaderboardSize)
                .ToList();
        }
    }
}