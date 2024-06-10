using System;
using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Represebts the current vital statistics of some game entity.
    /// </summary>
    public class Health : MonoBehaviour
    {
        public int maxHP = 3;

        public bool IsAlive => currentHP > 0;

        int currentHP;

        public void Increment()
        {
            currentHP = Mathf.Clamp(currentHP + 1, 0, maxHP);
        }

        public void Decrement(int DamageValue)
        {
            currentHP = Mathf.Clamp(currentHP - DamageValue, 0, maxHP);
            if (currentHP == 0)
            {
                var ev = Schedule<HealthIsZero>();
                ev.health = this;
            }
        }

        public void Die()
        {
            while (currentHP > 0) Decrement(maxHP);
        }

        void Awake()
        {
            currentHP = maxHP;
        }

        public void Respawn()
        {
            currentHP = maxHP;
        }

        public int GetHP()
        {
            return currentHP;
        }
    }
}
