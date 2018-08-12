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
        public static int mapTimerDays = 7;
        public static int mapTimerDaysmin = 0;
        public static int mapTimerDaysmax = 31;
        public static int timeout = 60;
        public static int timeoutmin = 0;
        public static int timeoutmax = 181;
        public static bool permanentCamps = false;
        public static bool homeEvents = false;
        public static bool caravanEvents = false;
        public static IntRange allowedSizeRange = new IntRange(50, 300);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<IntRange>(ref allowedSizeRange, "allowedSizeRange", new IntRange(75, 125), true);
            Scribe_Values.Look<int>(ref mapTimerDays, "mapTimerDays", 7, true);
            Scribe_Values.Look<int>(ref timeout, "Timeout", 60, true);
            Scribe_Values.Look<bool>(ref permanentCamps, "permanentCamps", false, true);
            Scribe_Values.Look<bool>(ref homeEvents, "homeEvents", false, true);
            Scribe_Values.Look<bool>(ref caravanEvents, "caravanEvents", false, true);
        }
    }
}
