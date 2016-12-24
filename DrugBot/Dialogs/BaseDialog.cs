using DrugBot.Common;
using DrugBot.Data;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DrugBot.Dialogs
{
    [Serializable]
    public class BaseDialog
    {
        protected Dictionary<int, int> GetDrugPrices(IDialogContext context)
        {
            var drugPrices = new Dictionary<int, int>();

            var db = new DrugBotDataContext();
            var drugs = db.GetDrugs().ToList();

            if (!context.UserData.TryGetValue(StateKeys.DrugPrices, out drugPrices))
            {
                drugPrices = new Dictionary<int, int>();

                var rand = new Random();

                foreach (var drug in drugs)
                {
                    var price = rand.Next(drug.MinPrice, drug.MaxPrice);
                    drugPrices.Add(drug.DrugId, price);
                }

                // store to state
                context.UserData.SetValue(StateKeys.DrugPrices, drugPrices);
            }

            return drugPrices;
        }
    }
}