using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace DrugBot.Data
{
    public class InventoryItem
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int InventoryItemId { get; set; }

        /// <summary>
        /// Current inventory quantity
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Navigation property for the related drug
        /// </summary>
        [ForeignKey("Drug")]
        public int DrugId { get; set; }
        public virtual Drug Drug { get; set; }

        /// <summary>
        /// Navigation property for the owner
        /// </summary>
        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }

    }
}