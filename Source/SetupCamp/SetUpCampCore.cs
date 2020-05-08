using HarmonyLib;
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
        }

        public override string SettingsCategory() => "SetUpCampSettingsCategoryLabel".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            checked
            {
                if (cachedMapTimerDays == -1)
                {
                    cachedMapTimerDays = SetUpCampSettings.mapTimerDays;
                }
                Listing_Standard listing_Standard = new Listing_Standard();
                listing_Standard.Begin(inRect);
                listing_Standard.Label("SetUpCampSettingMapSize".Translate());
                listing_Standard.IntRange(ref SetUpCampSettings.allowedSizeRange, 50, 300);
                if (SetUpCampSettings.mapTimerDays == SetUpCampSettings.mapTimerDaysmin)
                {
                    listing_Standard.Label("SetUpCampSettingsMapTimerDays".Translate() + ": OFF", -1, "SetUpCampSettingsMapTimerDaysTooltip".Translate());
                }
                else
                {
                    listing_Standard.Label("SetUpCampSettingsMapTimerDays".Translate() + ": " + SetUpCampSettings.mapTimerDays +" " + "Days".Translate(), -1, "SetUpCampSettingsMapTimerDaysTooltip".Translate());
                }
                SetUpCampSettings.mapTimerDays = (int)listing_Standard.Slider(SetUpCampSettings.mapTimerDays, SetUpCampSettings.mapTimerDaysmin, SetUpCampSettings.mapTimerDaysmax);
                if (SetUpCampSettings.timeout == SetUpCampSettings.timeoutmin)
                {
                    listing_Standard.Label("SetUpCampSettingsTimeout".Translate() + ": OFF", -1, "SetUpCampSettingsTimeoutTooltip".Translate());
                }
                else
                {
                    listing_Standard.Label("SetUpCampSettingsTimeout".Translate() + ": " + SetUpCampSettings.timeout + " " + "Days".Translate(), -1, "SetUpCampSettingsTimeoutTooltip".Translate());
                }
                SetUpCampSettings.timeout = (int)listing_Standard.Slider(SetUpCampSettings.timeout, SetUpCampSettings.timeoutmin, SetUpCampSettings.timeoutmax);
                listing_Standard.AddLabeledCheckbox("SetUpCampSettingsCustomMapGenDef".Translate() + ": ", ref SetUpCampSettings.customMapGenDef);
                listing_Standard.AddLabeledCheckbox("SetUpCampSettingsPermanentCamps".Translate() + ": ", ref SetUpCampSettings.permanentCamps);
                listing_Standard.AddLabeledCheckbox("SetUpCampSettingsHomeEvents".Translate() + ": ", ref SetUpCampSettings.homeEvents);
                listing_Standard.AddLabeledCheckbox("SetUpCampSettingsCaravanEvents".Translate() + ": ", ref SetUpCampSettings.caravanEvents);
                listing_Standard.End();
                settings.Write();
            }
        }

        public int cachedMapTimerDays = -1;

        public override void WriteSettings()
        {
            base.WriteSettings();
            if (SetUpCampSettings.mapTimerDays > 0)
            {
                int delta = (SetUpCampSettings.mapTimerDays - cachedMapTimerDays) * 60000;
                if (Current.ProgramState == ProgramState.Playing && delta > 0)
                {
                    foreach (CaravanCamp camp in Find.WorldObjects.AllWorldObjects.Where(wo => wo.def == SetUpCampDefOf.CaravanCamp))
                    {
                        camp.ChangeTimer(delta);
                    }
                }
            }
            else
            {
                if (Current.ProgramState == ProgramState.Playing)
                {
                    foreach (CaravanCamp camp in Find.WorldObjects.AllWorldObjects.Where(wo => wo.def == SetUpCampDefOf.CaravanCamp))
                    {
                        camp.GetComponent<TimedForcedExit>().ResetForceExitAndRemoveMapCountdown();
                        camp.startedCountdown = false;
                    }
                }
            }
            cachedMapTimerDays = SetUpCampSettings.mapTimerDays;
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

    public class SetUpCampSettings : ModSettings
    {
        private const int MinSizeDefault = 75;
        private const int MaxSizeDefault = 125;
        public static int mapTimerDays = 7;
        public static int mapTimerDaysmin = 0;
        public static int mapTimerDaysmax = 30;
        public static int timeout = 60;
        public static int timeoutmin = 0;
        public static int timeoutmax = 180;
        public static bool customMapGenDef = true;
        public static bool permanentCamps = false;
        public static bool homeEvents = false;
        public static bool caravanEvents = false;
        public static IntRange allowedSizeRange = new IntRange(75, 125);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<IntRange>(ref allowedSizeRange, "allowedSizeRange", new IntRange(75, 125), true);
            Scribe_Values.Look<int>(ref mapTimerDays, "mapTimerDays", 7, true);
            Scribe_Values.Look<int>(ref timeout, "Timeout", 60, true);
            Scribe_Values.Look<bool>(ref customMapGenDef, "customMapGenDef", true, true);
            Scribe_Values.Look<bool>(ref permanentCamps, "permanentCamps", false, true);
            Scribe_Values.Look<bool>(ref homeEvents, "homeEvents", false, true);
            Scribe_Values.Look<bool>(ref caravanEvents, "caravanEvents", false, true);
        }
    }
}
