using System;
using UnityEngine;
using Verse;

namespace SetUpCamp
{
    // Token: 0x02000005 RID: 5
    [StaticConstructorOnStartup]
    public class SetUpCampTextures
    {
        // Token: 0x0400000E RID: 14
        public static readonly Texture2D CampCommandTex = ContentFinder<Texture2D>.Get("SetupCamp/Flag", true);

        // Token: 0x0400000F RID: 15
        public static readonly Texture2D CampCommandTexGears = ContentFinder<Texture2D>.Get("SetupCamp/FlagGears", true);

        // Token: 0x04000010 RID: 16
        public static readonly Texture2D CampCommandTexDelete = ContentFinder<Texture2D>.Get("SetupCamp/Delete", true);
    }
}
