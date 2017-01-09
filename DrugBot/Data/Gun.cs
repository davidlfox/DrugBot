using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DrugBot.Data
{
    public class Gun
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int GunId { get; set; }

        /// <summary>
        /// Name of the gun
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Damage per shot
        /// </summary>
        public int Damage { get; set; }

        /// <summary>
        /// Some kind of estimate on the price of the weapon
        /// </summary>
        public int Cost { get; set; }

        /// <summary>
        /// Minimum day-of-game required to have a chance to pop this weapon
        /// </summary>
        public int MinimumDayOfGame { get; set; }
    }
}