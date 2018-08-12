using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using System.Reflection;
using RimWorld.Planet;
using Verse.Sound;
using UnityEngine;

namespace Syrchalis_SetUpCamp
{
    public class SetUpCamp : Mod
    {
        public static SetUpCampSettings settings;

        public SetUpCamp(ModContentPack content) : base(content)
        {
            settings = GetSettings<SetUpCampSettings>();
            var harmony = HarmonyInstance.Create("Syrchalis.Rimworld.SetUpCamp");
            MethodInfo method = typeof(Caravan).GetMethod("GetGizmos");
            HarmonyMethod prefix = null;
            HarmonyMethod postfix = new HarmonyMethod(typeof(SetUpCamp).GetMethod("GetGizmosPostfix")); ;
            harmony.Patch(method, prefix, postfix, null);
#if DEBUG
            Log.Message("[SetUpCamp] loaded...");
#endif
        }

        public override string SettingsCategory() => "SetUpCampSettingsCategoryLabel".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            checked
            {
                Listing_Standard listing_Standard = new Listing_Standard();
                listing_Standard.Begin(inRect);
                listing_Standard.Label("SetUpCampSettingMapSize".Translate());
                listing_Standard.IntRange(ref SetUpCampSettings.allowedSizeRange, 50, 300);

                if (SetUpCampSettings.mapTimerDays == 0 || SetUpCampSettings.mapTimerDays == 31)
                {
                    listing_Standard.Label("SetUpCampSettingsMapTimerDays".Translate() + ": OFF", -1, "SetUpCampSettingsMapTimerDaysTooltip".Translate());
                }
                else
                {
                    listing_Standard.Label("SetUpCampSettingsMapTimerDays".Translate() + ": " + SetUpCampSettings.mapTimerDays +" " + "Days".Translate(), -1, "SetUpCampSettingsMapTimerDaysTooltip".Translate());
                }
                SetUpCampSettings.mapTimerDays = (int)listing_Standard.Slider(SetUpCampSettings.mapTimerDays, SetUpCampSettings.mapTimerDaysmin, SetUpCampSettings.mapTimerDaysmax);

                if (SetUpCampSettings.timeout == SetUpCampSettings.timeoutmin || SetUpCampSettings.timeout == SetUpCampSettings.timeoutmax)
                {
                    listing_Standard.Label("SetUpCampSettingsTimeout".Translate() + ": OFF", -1, "SetUpCampSettingsTimeoutTooltip".Translate());
                }
                else
                {
                    listing_Standard.Label("SetUpCampSettingsTimeout".Translate() + ": " + SetUpCampSettings.timeout + " " + "Days".Translate(), -1, "SetUpCampSettingsTimeoutTooltip".Translate());
                }
                SetUpCampSettings.timeout = (int)listing_Standard.Slider(SetUpCampSettings.timeout, SetUpCampSettings.timeoutmin, SetUpCampSettings.timeoutmax);
                listing_Standard.AddLabeledCheckbox("SetUpCampSettingsPermanentCamps".Translate() + ": ", ref SetUpCampSettings.permanentCamps);
                listing_Standard.AddLabeledCheckbox("SetUpCampSettingsHomeEvents".Translate() + ": ", ref SetUpCampSettings.homeEvents);
                listing_Standard.AddLabeledCheckbox("SetUpCampSettingsCaravanEvents".Translate() + ": ", ref SetUpCampSettings.caravanEvents);
                listing_Standard.End();
                settings.Write();
            }
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
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

        private static List<Pawn> tmpCaravanPawns = new List<Pawn>();
        public static void Camp(Caravan caravan)
        {
            float PawnScore = CaravanPawnsCountScore(caravan);
            string randomSeed = new System.Random().Next(1, 99999999).ToString();
            int CampSize = new System.Random().Next(SetUpCampSettings.allowedSizeRange.min, SetUpCampSettings.allowedSizeRange.max);
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
