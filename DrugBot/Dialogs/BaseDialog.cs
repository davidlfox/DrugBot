using DrugBot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DrugBot.Dialogs
{
    public class BaseDialog
    {
        private DrugBotDataContext _db;

        public BaseDialog()
        {
            this._db = new DrugBotDataContext();
        }

        public int Commit()
        {
            return this._db.SaveChanges();
        }

        public User FindUser(string botUserId)
        {
            return this._db.Users.FirstOrDefault(x => x.BotUserId == botUserId);
        }

        public User AddUser(User user)
        {
            this._db.Users.Add(user);
            return user;
        }

        public User AddUser(string botUserID, string name, int wallet)
        {
            var user = new User
            {
                BotUserId = botUserID,
                Name = name,
                Wallet = wallet,
            };

            this.AddUser(user);

            return user;
        }

        public Drug FindDrug(string name)
        {
            return this._db.Drugs.FirstOrDefault(x => x.Name == name);
        }

        public IQueryable<Drug> GetDrugs()
        {
            return this._db.Drugs;
        }
    }
}