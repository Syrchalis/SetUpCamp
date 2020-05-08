using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;
using Verse.AI;
using Verse.Sound;

namespace Syrchalis_SetUpCamp
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("Syrchalis.Rimworld.SetUpCamp");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(Caravan), nameof(Caravan.GetGizmos))]
    public static class GetGizmosPatch
    {
        [HarmonyPostfix]
        public static IEnumerable<Gizmo> GetGizmosPostfix(IEnumerable<Gizmo> __result, Caravan __instance)
        {
            foreach (Gizmo gizmo in __result)
            {
                yield return gizmo;
            }
            yield return SetUpCamp_Utility.SetUpCampCommand(__instance);
        }
    }
}
