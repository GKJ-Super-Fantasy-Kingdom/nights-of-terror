using System;
using System.Collections.Generic;
using SuperFantasyKingdom;
using SuperFantasyKingdom.Spawner;

namespace NightsOfTerror
{
    internal sealed class DuskPool
    {
        private readonly List<(Monster Prefab, int Difficulty)> enemies = new List<(Monster, int)>();

        internal DuskPool(MonsterSpawner.Wave[] waves, int currentDifficulty)
        {
            if (waves == null) return;
            int eligibleTiers = Math.Min(currentDifficulty, waves.Length);
            for (int tier = 0; tier < eligibleTiers; tier++)
            {
                var prefab = waves[tier]?.monster;
                if (prefab != null && !prefab.IsBoss() && !prefab.IsMiniBoss())
                    enemies.Add((prefab, tier + 1));
            }
        }

        internal int Count => enemies.Count;

        // Native ordinary waves halve the odds for each older tier. The oldest
        // eligible tier receives the remainder. Empty slots are skipped.
        internal (Monster Prefab, int Difficulty) Pick(double roll)
        {
            int index = enemies.Count - 1;
            while (index > 0 && roll < 0.5)
            {
                index--;
                roll *= 2;
            }
            return enemies[index];
        }
    }
}
