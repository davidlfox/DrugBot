using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DrugBot.Data
{
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
}