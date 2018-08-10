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
    public class SetUpCampSettings : ModSettings
    {
        private const int MinSizeDefault = 75;
        private const int MaxSizeDefault = 125;
        public static bool permanentCamps = false;
        public static bool homeEvents = false;
        public static bool caravanEvents = false;
        public static IntRange allowedSizeRange = new IntRange(50, 300);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<IntRange>(ref allowedSizeRange, "allowedSizeRange", new IntRange(75, 125), true);
            Scribe_Values.Look<bool>(ref permanentCamps, "permanentCamps", false, true);
            Scribe_Values.Look<bool>(ref homeEvents, "homeEvents", false, true);
            Scribe_Values.Look<bool>(ref caravanEvents, "caravanEvents", false, true);
        }
    }

    [DefOf]
    public static class SetUpCampDefOf
    {
        static SetUpCampDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(SetUpCampDefOf));
        }
        public static WorldObjectDef CaravanCamp;
    }

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
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            listing_Standard.Label("SetUpCampSettingMapSize".Translate());
            listing_Standard.IntRange(ref SetUpCampSettings.allowedSizeRange, 50, 300);
            listing_Standard.AddLabeledCheckbox("SetUpCampSettingsPermanentCamps".Translate() + ": ", ref SetUpCampSettings.permanentCamps);
            listing_Standard.AddLabeledCheckbox("SetUpCampSettingsHomeEvents".Translate() + ": ", ref SetUpCampSettings.homeEvents);
            listing_Standard.AddLabeledCheckbox("SetUpCampSettingsCaravanEvents".Translate() + ": ", ref SetUpCampSettings.caravanEvents);
            listing_Standard.End();
            settings.Write();
        }

        public static void ApplySettings()
        {
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

        public override void WriteSettings()
        {
            base.WriteSettings();
            ApplySettings();
        }

        public static void GetGizmosPostfix(ref IEnumerable<Gizmo> __result, Caravan __instance)
        {
            __result = __result.Concat(new Gizmo[] { SetUpCampCommand(__instance) });
            ApplySettings();
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

            if (!TileFinder.IsValidTileForNewSettlement(caravan.Tile, tmpSettleFailReason))
            {
                command_Action.Disable(tmpSettleFailReason.ToString());
            }
            return command_Action;
        }

        private static List<Pawn> tmpCaravanPawns = new List<Pawn>();
        public static void Camp(Caravan caravan)
        {
            float PawnScore = CaravanPawnsCountScore(caravan);
            //int num2 = Mathf.RoundToInt(PawnScore * 900f);
            int CampSize = new System.Random().Next(SetUpCampSettings.allowedSizeRange.min, SetUpCampSettings.allowedSizeRange.max);
            //float num4 = Mathf.Max(PawnScore * EnemyPointsPerCaravanPawnsScoreRange.RandomInRange, 45f);
            Pawn pawn = caravan.PawnsListForReading[0];
            tmpCaravanPawns.Clear();
            tmpCaravanPawns.AddRange(caravan.PawnsListForReading);
            Map map = CaravanIncidentUtility.GetOrGenerateMapForIncident(caravan, new IntVec3(CampSize, 1, CampSize), DefDatabase<WorldObjectDef>.GetNamed("CaravanCamp", true));
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
