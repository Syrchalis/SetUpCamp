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
    [DefOf]
    public static class SetUpCampDefOf
    {
        static SetUpCampDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(SetUpCampDefOf));
        }
        public static WorldObjectDef CaravanCamp;
        public static WorldObjectDef AbandonedCamp;
        public static MapGeneratorDef Syr_SetUpCamp;
        public static MapGeneratorDef Syr_SetUpCampNR;
    }
}
