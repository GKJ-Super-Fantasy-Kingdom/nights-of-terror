using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using SuperFantasyKingdom;
using SuperFantasyKingdom.Spawner;
using UnityEngine;

namespace NightsOfTerror
{
    [BepInPlugin("gkj.nightsofterror", "Nights of Terror", "0.1.0")]
    public sealed class Plugin : BaseUnityPlugin
    {
        internal static Plugin Active;
        private Harmony patches;
        private ConfigEntry<bool> rushed;
        private DaytimeManager clock;
        private Monster prefab;
        private int night;
        internal bool Settled { get; private set; }
        private Reinforcement group;
        private Action<MonsterSpawner, Monster, int, Vector3> spawn;

        private void Awake()
        {
            rushed = Config.Bind("Reinforcements", "RushedArrival", true,
                "Use the native Rush spacing for this group. When disabled, its members arrive one second apart.");
            patches = new Harmony("gkj.nightsofterror");
            try
            {
                var method = AccessTools.DeclaredMethod(typeof(MonsterSpawner), "SpawnMonster",
                    new[] { typeof(Monster), typeof(int), typeof(Vector3) });
                spawn = AccessTools.MethodDelegate<Action<MonsterSpawner, Monster, int, Vector3>>(method);
                patches.PatchAll(typeof(Plugin).Assembly);
            }
            catch (Exception error)
            {
                patches.UnpatchSelf();
                Logger.LogError("Could not install the native wave hooks. " + error);
                return;
            }
            Active = this;
            GameManager.OnGameInit += ResetNight;
            GameManager.OnGameOver += ResetNight;
            DaytimeManager.BeforeMorningStart += Dawn;
            DaytimeManager.OnSunsetStart += Dusk;
        }

        private void OnDestroy()
        {
            Active = null;
            GameManager.OnGameInit -= ResetNight;
            GameManager.OnGameOver -= ResetNight;
            DaytimeManager.BeforeMorningStart -= Dawn;
            DaytimeManager.OnSunsetStart -= Dusk;
            patches?.UnpatchSelf();
        }

        private void Dawn(int day) => ResetNight();

        private void ResetNight()
        {
            clock = null;
            prefab = null;
            night = 0;
            Settled = false;
            group = null;
        }

        internal bool BeginSettlement()
        {
            if (Settled) return false;
            Settled = true;
            return true;
        }

        private void Dusk(int day)
        {
            if (Schedule(DaytimeManager.Instance, MonsterSpawner.Instance, day))
                AlertManager.Instance.DisplayMessage("Nights of Terror", "Three more foes arrive at midnight.");
        }

        private bool Schedule(DaytimeManager today, MonsterSpawner spawner, int day)
        {
            if (today == null || spawner == null || Owns(today) || today.IsSpecialDay() || today.IsBossDay()
                || day >= today.GetDayLimit() || GameManager.Instance.IsGameOver()) return false;
            // An ordinary day's prepared slot has already supplied the main wave.
            var enemy = spawner.GetMonsterToSpawn(0);
            if (enemy == null || enemy.IsBoss() || enemy.IsMiniBoss()) return false;
            clock = today;
            night = day;
            group = new Reinforcement();
            prefab = enemy;
            return true;
        }

        internal bool Owns(DaytimeManager today) => group != null && ReferenceEquals(today, clock) && night == today.CurrentDay();

        internal int ForClearance(int living) => Owns(DaytimeManager.Instance) ? group.ForClearance(living) : living;

        // SwitchState runs when the scaled evening timer crosses zero, then on each
        // overtime tick until the real monsters, loot and tavern checks all clear.
        internal void EveningExpired(DaytimeManager today)
        {
            if (!Owns(today) || !today.IsEvening() || !group.IsDue(Time.time)) return;
            var spawner = MonsterSpawner.Instance;
            var edge = CombatManager.Instance.GetCombatZone() + Vector2.one * 0.1f;
            var pos = new Vector3(UnityEngine.Random.Range(-edge.x, edge.x), edge.y, 0f);
            bool first = group.Remaining == Reinforcement.GroupSize;
            // Keep the reservation until native Init and MonsterSpawn have returned.
            // If a native observer throws after registration, never retry the group.
            try
            {
                spawn(spawner, prefab, spawner.GetCurrentDifficulty(), pos);
                group.Arrived(Time.time, rushed.Value);
            }
            catch (Exception error)
            {
                group.Cancel();
                Logger.LogError("Reinforcement interrupted. Native combat will settle surviving enemies. " + error);
                return;
            }
            if (first) AlertManager.Instance.DisplayMessage("Midnight", "Reinforcements have arrived!");
        }
    }
}
