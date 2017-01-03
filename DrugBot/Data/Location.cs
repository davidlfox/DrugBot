using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DrugBot.Data
{
    public class Location
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int LocationId { get; set; }

        /// <summary>
        /// Name of the location
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Some kind of description of the location
        /// </summary>
        public string Description { get; set; }
    }
}