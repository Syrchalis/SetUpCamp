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
    public class GetGizmosPatch
    {
        [HarmonyPostfix]
        public static void GetGizmosPostfix(ref IEnumerable<Gizmo> __result, Caravan __instance)
        {
            __result = __result.Concat(new Gizmo[] { SetUpCampCommand(__instance) });
        }

        private static StringBuilder tmpSettleFailReason = new StringBuilder();
        public static Command SetUpCampCommand(Caravan caravan)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "SetUpCamp".Translate();
            command_Action.defaultDesc = "SetUpCampDesc".Translate();
            command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/SetUpCamp", true);
            command_Action.action = delegate ()
            {
                SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High, null);
                Camp(caravan);
            };
            tmpSettleFailReason.Length = 0;
            if (!TileFinder.IsValidTileForNewSettlement(caravan.Tile, tmpSettleFailReason))
            {
                command_Action.Disable(tmpSettleFailReason.ToString());
            }
            if (Find.WorldObjects.AnyWorldObjectOfDefAt(SetUpCampDefOf.AbandonedCamp, caravan.Tile))
            {
                command_Action.Disable("SetUpCampOccupied".Translate());
            }
            return command_Action;
        }

        private static List<Pawn> tmpCaravanPawns = new List<Pawn>();
        public static void Camp(Caravan caravan)
        {
            float PawnScore = CaravanPawnsCountScore(caravan);
            string randomSeed = Find.TickManager.TicksGame.ToString();
            int CampSize = Rand.Range(SetUpCampSettings.allowedSizeRange.min, SetUpCampSettings.allowedSizeRange.max);
            //float num4 = Mathf.Max(PawnScore * EnemyPointsPerCaravanPawnsScoreRange.RandomInRange, 45f);
            Pawn pawn = caravan.PawnsListForReading[0];
            tmpCaravanPawns.Clear();
            tmpCaravanPawns.AddRange(caravan.PawnsListForReading);
            Map map = GenerateNewMapWithSeed(caravan, new IntVec3(CampSize, 1, CampSize), randomSeed);
            MultipleCaravansCellFinder.FindStartingCellsFor2Groups(map, out IntVec3 playerStartingSpot, out IntVec3 intVec);
            CaravanEnterMapUtility.Enter(caravan, map, (Pawn x) => CellFinder.RandomSpawnCellForPawnNear(playerStartingSpot, map, 4), 0, true);
            tmpCaravanPawns.Clear();
            CameraJumper.TryJumpAndSelect(pawn);
            Find.TickManager.CurTimeSpeed = 0;
            Messages.Message("SetUpCampFormedCamp".Translate(), MessageTypeDefOf.PositiveEvent);
        }

        //Generates a map with a defined seed
        private static Map GenerateNewMapWithSeed(Caravan caravan, IntVec3 size, string seed)
        {
            int tile = caravan.Tile;
            bool flag = Current.Game.FindMap(tile) == null;
            string seedString = Find.World.info.seedString;
            Find.World.info.seedString = seed;
            Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(tile, size, DefDatabase<WorldObjectDef>.GetNamed("CaravanCamp", true));
            Find.World.info.seedString = seedString;
            if (flag && orGenerateMap != null)
            {
                orGenerateMap.retainedCaravanData.Notify_GeneratedTempIncidentMapFor(caravan);
            }
            return orGenerateMap;
        }

        private static float CaravanPawnsCountScore(Caravan caravan)
        {
            float num = 0f;
            List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
            for (int i = 0; i < pawnsListForReading.Count; i++)
            {
                bool isColonist = pawnsListForReading[i].IsColonist;
                if (isColonist)
                {
                    num += (pawnsListForReading[i].Downed ? 0.35f : 1f);
                }
                else
                {
                    bool humanlike = pawnsListForReading[i].RaceProps.Humanlike;
                    if (humanlike)
                    {
                        num += 0.35f;
                    }
                    else
                    {
                        num += 0.2f * pawnsListForReading[i].BodySize;
                    }
                }
            }
            return num;
        }
    }
}
