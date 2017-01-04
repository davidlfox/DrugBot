using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DrugBot.Data.Attributes
{
    public class UniqueUserNameAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var proposedName = value.ToString();

            var db = new DrugBotDataContext();

            return !db.Users.Any(x => x.Name == proposedName);
        }
    }
}