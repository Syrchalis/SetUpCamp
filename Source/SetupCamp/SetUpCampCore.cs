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
                listing_Standard.AddLabeledCheckbox("SetUpCampSettingsCustomMapGenDef".Translate() + ": ", ref SetUpCampSettings.customMapGenDef);
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
    }
}
