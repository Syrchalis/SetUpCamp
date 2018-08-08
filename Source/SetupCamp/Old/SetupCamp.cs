using System;
using UnityEngine;
using Verse;

namespace SetUpCamp
{
    // Token: 0x02000006 RID: 6
    [StaticConstructorOnStartup]
    public class SetUpCamp : Mod
    {
        // Token: 0x06000015 RID: 21 RVA: 0x000024F8 File Offset: 0x000006F8
        public SetUpCamp(ModContentPack content) : base(content)
        {
            base.GetSettings<SetUpCampSettings>();
        }

        // Token: 0x06000016 RID: 22 RVA: 0x0000250A File Offset: 0x0000070A
        public override void DoSettingsWindowContents(Rect inRect)
        {
            SetUpCampSettings.DoSettingsWindowContents(inRect);
        }

        // Token: 0x06000017 RID: 23 RVA: 0x00002514 File Offset: 0x00000714
        public override string SettingsCategory()
        {
            return "Set-Up Camp";
        }

        // Token: 0x06000018 RID: 24 RVA: 0x0000252B File Offset: 0x0000072B
        public void Save()
        {
            LoadedModManager.GetMod<SetUpCamp>().GetSettings<SetUpCampSettings>().Write();
        }
    }
}
