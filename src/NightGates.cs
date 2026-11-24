using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using SuperFantasyKingdom;

namespace NightsOfTerror
{
    internal static class NightGates
    {
        [HarmonyPatch(typeof(DaytimeManager), "SwitchState")]
        private static class PhaseAdvance
        {
            private static void Prefix(DaytimeManager __instance) => Plugin.Active?.EveningExpired(__instance);
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                => GateClearance(instructions, CountBeforeNight);
        }

        [HarmonyPatch(typeof(DaytimeManager), "Update")]
        private static class TavernPrompt
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                => GateClearance(instructions, CountBeforeTavern);
        }

        // Only the two native clearance decisions need to wait between arrivals.
        // The combat registry, target lists and every other caller see actual enemies.
        private static IEnumerable<CodeInstruction> GateClearance(IEnumerable<CodeInstruction> instructions, Func<int, int> gate)
        {
            var code = instructions.ToList();
            var count = typeof(CombatManager).GetMethod(nameof(CombatManager.CurrentMonstersAmount));
            var sites = code.FindAll(i => i.Calls(count));
            if (sites.Count != 1) throw new InvalidOperationException("Expected one native night-clearance check.");
            code.Insert(code.IndexOf(sites[0]) + 1, new CodeInstruction(OpCodes.Call, gate.Method));
            return code;
        }

        private static int CountBeforeTavern(int actual)
            => Plugin.Active?.ForClearance(actual) ?? actual;

        // Update publishes CombatEnd after clearing pending enemies, chests and
        // provocations. SwitchState must observe that signal before auto-tavern
        // can enter Night and disable the clock on its final tick.
        private static int CountBeforeNight(int actual)
            => actual == 0 && Plugin.Active?.Owns(DaytimeManager.Instance) == true &&
               !CombatManager.Instance.IsCombatOver() ? 1 : actual;

        // Manual entry and the auto-skip timer can both request Survived during
        // the scene transition. Claim it before BeforeStateChange pays wagers.
        [HarmonyPatch(typeof(GameManager), nameof(GameManager.ChangeState))]
        private static class Survival
        {
            private static bool Prefix(GameState __0) => __0 != GameState.Survived ||
                Plugin.Active?.Owns(DaytimeManager.Instance) != true || (CombatManager.Instance.IsCombatOver() && Plugin.Active.BeginSettlement());
        }

        [HarmonyPatch(typeof(GameManager), nameof(GameManager.EndDay))]
        private static class TavernEntry
        {
            private static bool Prefix() => Plugin.Active?.Owns(DaytimeManager.Instance) != true
                || (!Plugin.Active.Settled && CombatManager.Instance.IsCombatOver());
        }
    }
}
