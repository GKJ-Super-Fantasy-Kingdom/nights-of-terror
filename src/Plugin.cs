using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using SuperFantasyKingdom;
using SuperFantasyKingdom.Spawner;
using UnityEngine;

namespace NightsOfTerror
{
    [BepInPlugin("gkj.nightsofterror", "Nights of Terror", "0.1.2")]
    public sealed class Plugin : BaseUnityPlugin
    {
        internal static Plugin Active;
        private Harmony patches;
        private ConfigEntry<bool> rushed;
        private DaytimeManager clock;
        private DuskPool pool;
        private AccessTools.FieldRef<MonsterSpawner, int> ordinaryQuota;
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
                ordinaryQuota = AccessTools.FieldRefAccess<MonsterSpawner, int>("m_MinMonstersToSpawn");
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
            pool = null;
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
            var spawner = MonsterSpawner.Instance;
            if (spawner == null) return;
            int quota = ordinaryQuota(spawner);
            if (!Schedule(DaytimeManager.Instance, spawner, day, quota)) return;
            Logger.LogInfo($"Night {day}: dusk quota {quota}, remaining {spawner.GetMonstersToSpawn()}, midnight group {group.Count}, eligible enemy tiers {pool.Count}.");
            AlertManager.Instance.DisplayMessage("Nights of Terror", $"{group.Count} more foes arrive at midnight.");
        }

        private bool Schedule(DaytimeManager today, MonsterSpawner spawner, int day, int quota)
        {
            if (today == null || spawner == null || Owns(today) || today.IsSpecialDay() || today.IsBossDay()
                || day >= today.GetDayLimit() || GameManager.Instance.IsGameOver()) return false;
            int count = Reinforcement.SizeFromQuota(quota);
            if (count == 0) return false;
            // On ordinary nights the native difficulty is the current wave index plus one.
            var selected = new DuskPool(spawner.waves, spawner.GetCurrentDifficulty());
            if (selected.Count == 0) return false;
            clock = today;
            night = day;
            group = new Reinforcement(count);
            pool = selected;
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
            bool first = group.Remaining == group.Count;
            // Keep the reservation until native Init and MonsterSpawn have returned.
            // If a native observer throws after registration, never retry the group.
            try
            {
                var enemy = pool.Pick(UnityEngine.Random.value);
                spawn(spawner, enemy.Prefab, enemy.Difficulty, pos);
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
