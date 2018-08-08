using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace SetUpCamp
{
    // Token: 0x02000003 RID: 3
    public abstract class CaravanCampUtility : IncidentWorker
    {
        // Token: 0x06000008 RID: 8
        protected abstract List<Pawn> GeneratePawns(Caravan caravan, float points, Map map);

        // Token: 0x06000009 RID: 9 RVA: 0x00002186 File Offset: 0x00000386
        private void PostProcessGeneratedPawnsAfterSpawning(List<Pawn> generatedPawns)
        {
        }

        // Token: 0x0600000A RID: 10 RVA: 0x0000218C File Offset: 0x0000038C
        public static void Camp(Caravan caravan)
        {
            SetUpCampSettings settings = LoadedModManager.GetMod<SetUpCamp>().GetSettings<SetUpCampSettings>();
            int minSize = SetUpCampSettings.MinSize;
            int maxSize = SetUpCampSettings.MaxSize;
            float num = CaravanPawnsCountScore(caravan);
            int num2 = Mathf.RoundToInt(num * 900f);
            int num3 = new System.Random().Next(minSize, maxSize);
            float num4 = Mathf.Max(num * EnemyPointsPerCaravanPawnsScoreRange.RandomInRange, 45f);
            Pawn pawn = caravan.PawnsListForReading[0];
            tmpCaravanPawns.Clear();
            tmpCaravanPawns.AddRange(caravan.PawnsListForReading);
            Map map = CaravanIncidentUtility.GetOrGenerateMapForIncident(caravan, new IntVec3(num3, 1, num3), DefDatabase<WorldObjectDef>.GetNamed("CaravanCamp", true));
            MultipleCaravansCellFinder.FindStartingCellsFor2Groups(map, out IntVec3 playerStartingSpot, out IntVec3 intVec);
            CaravanEnterMapUtility.Enter(caravan, map, (Pawn x) => CellFinder.RandomSpawnCellForPawnNear(playerStartingSpot, map, 4), 0, true);
            tmpCaravanPawns.Clear();
            CameraJumper.TryJumpAndSelect(pawn);
            Find.TickManager.CurTimeSpeed = 0;
            Messages.Message("The caravan formed a camp.", MessageTypeDefOf.TaskCompletion);
        }

        // Token: 0x0600000B RID: 11 RVA: 0x000022A8 File Offset: 0x000004A8
        public static Command CampCommand(Caravan caravan)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "Set-Up Camp";
            command_Action.defaultDesc = "Set-Up Camp";
            command_Action.icon = SetUpCampTextures.CampCommandTex;
            command_Action.action = delegate()
            {
                SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High, null);
                Camp(caravan);
            };
            bool flag = false;
            List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
            for (int i = 0; i < allWorldObjects.Count; i++)
            {
                WorldObject worldObject = allWorldObjects[i];
                bool flag2 = worldObject.Tile == caravan.Tile && worldObject != caravan;
                if (flag2)
                {
                    flag = true;
                    break;
                }
            }
            bool flag3 = flag;
            if (flag3)
            {
                command_Action.Disable(Translator.Translate("CommandSettleFailOtherWorldObjectsHere"));
            }
            return command_Action;
        }

        // Token: 0x0600000C RID: 12 RVA: 0x00002380 File Offset: 0x00000580
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

        // Token: 0x04000002 RID: 2
        private const int MinStartingLocCellsCount = 600;

        // Token: 0x04000003 RID: 3
        private const float CaravanPawnsScorePerColonistUndowned = 1f;

        // Token: 0x04000004 RID: 4
        private const float CaravanPawnsScorePerColonistDowned = 0.35f;

        // Token: 0x04000005 RID: 5
        private const float CaravanPawnsScorePerPrisoner = 0.35f;

        // Token: 0x04000006 RID: 6
        public const float CaravanPawnsScorePerAnimalBodySize = 0.2f;

        // Token: 0x04000007 RID: 7
        private const int MapCellsPerCaravanPawnsCountScore = 900;

        // Token: 0x04000008 RID: 8
        private const float MinEnemyPoints = 45f;

        // Token: 0x04000009 RID: 9
        private static List<Pawn> tmpCaravanPawns = new List<Pawn>();

        // Token: 0x0400000A RID: 10
        private static readonly FloatRange EnemyPointsPerCaravanPawnsScoreRange = new FloatRange(35f, 90f);
    }
}
