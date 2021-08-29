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
            if (SetUpCampSettings.homeEvents)
            {
                SetUpCampDefOf.CaravanCamp.IncidentTargetTags.Add(IncidentTargetTagDefOf.Map_PlayerHome);
            }
            else
            {
                SetUpCampDefOf.CaravanCamp.IncidentTargetTags.Remove(IncidentTargetTagDefOf.Map_PlayerHome);
            }
            if (SetUpCampSettings.caravanEvents)
            {
                SetUpCampDefOf.CaravanCamp.IncidentTargetTags.Add(IncidentTargetTagDefOf.Caravan);
            }
            else
            {
                SetUpCampDefOf.CaravanCamp.IncidentTargetTags.Remove(IncidentTargetTagDefOf.Caravan);
            }
            SetUpCampDefOf.CaravanCamp.ResolveReferences();
        }
    }

    [HarmonyPatch(typeof(AnimalPenUtility), "NeedsToBeManagedByRope")]
    class Patch
    {
        [HarmonyPrefix]
        static bool Prefix(Pawn pawn, ref bool __result)
        {
            __result = AnimalPenUtility.IsRopeManagedAnimalDef(pawn.def) && pawn.Spawned;
            return false;
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
