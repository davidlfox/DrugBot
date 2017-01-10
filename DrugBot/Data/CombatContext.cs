using DrugBot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DrugBot.Data
{
    public class CombatContext
    {
        public int PlayerHitPoints { get; set; }
        public int EnemyHitPoints { get; set; }

        public CombatContext()
        {
            this.PlayerHitPoints = 100;
            this.EnemyHitPoints = RandomEvent.GetRandomNumberBetween(30, 60);
        }

        public void HitEnemy(int damage)
        {
            this.EnemyHitPoints -= damage;
        }

        public void HitPlayer(int damage)
        {
            this.PlayerHitPoints -= damage;
        }
    }
}