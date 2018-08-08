using System;
using UnityEngine;
using Verse;

namespace SetUpCamp
{
    // Token: 0x02000007 RID: 7
    public class SetUpCampSettings : ModSettings
    {
        // Token: 0x06000019 RID: 25 RVA: 0x00002540 File Offset: 0x00000740
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref SetUpCampSettings.MinSize, "MinSize", 75, false);
            Scribe_Values.Look<int>(ref SetUpCampSettings.MaxSize, "MaxSize", 90, false);
            Scribe_Values.Look<bool>(ref SetUpCampSettings.permanentCamps, "permanentCamps", false, false);
        }

        // Token: 0x0600001A RID: 26 RVA: 0x00002590 File Offset: 0x00000790
        public static void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            listing_Standard.Label("Min. Camp Size: " + SetUpCampSettings.MinSize.ToString(), -1f);
            SetUpCampSettings.MinSize = (int)listing_Standard.Slider((float)SetUpCampSettings.MinSize, 25f, 350f);
            listing_Standard.Label("Max. Camp Size: " + SetUpCampSettings.MaxSize.ToString(), -1f);
            SetUpCampSettings.MaxSize = (int)listing_Standard.Slider((float)SetUpCampSettings.MaxSize, 25f, 350f);
            listing_Standard.CheckboxLabeled("Permanent Camps (!)", ref SetUpCampSettings.permanentCamps, "WARNING: Disabling will delete any empty permanent camps on the map!");
            listing_Standard.End();
        }

        // Token: 0x04000011 RID: 17
        public static int MaxSize = 90;

        // Token: 0x04000012 RID: 18
        public static int MinSize = 75;

        // Token: 0x04000013 RID: 19
        public static bool permanentCamps = false;
    }
}
