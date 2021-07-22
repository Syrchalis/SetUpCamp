using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;
using Verse.AI;
using Verse.Sound;

namespace Syrchalis_SetUpCamp
{
    [StaticConstructorOnStartup]
    public static class SetUpCamp_Utility
    {
        public static readonly Texture2D SetUpCampCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/SetUpCamp", true);
        private static StringBuilder tmpSettleFailReason = new StringBuilder();
        
        public static Command_Action SetUpCampCommand(Caravan caravan)
        {
            Command_Action command_Action = new Command_Action
            {
                defaultLabel = "SetUpCamp".Translate(),
                defaultDesc = "SetUpCampDesc".Translate(),
                icon = SetUpCampCommandTex,
                action = delegate ()
                {
                    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High, null);
                    Camp(caravan);
                }
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

        public static void Camp(Caravan caravan)
        {
            Faction faction = caravan.Faction;
            if (faction != Faction.OfPlayer)
            {
                Log.Error("Cannot camp with non-player faction.", false);
                return;
            }
            string randomSeed = Find.TickManager.TicksAbs.ToString();
            int CampSize = Rand.Range(SetUpCampSettings.allowedSizeRange.min, SetUpCampSettings.allowedSizeRange.max);
            CaravanCamp camp = NewCaravanCamp(caravan.Tile, faction);
            LongEventHandler.QueueLongEvent(delegate ()
            {
                GenerateNewMapWithSeed(caravan.Tile, new IntVec3(CampSize, 1, CampSize), randomSeed);
            }, "SetUpCamp_GeneratingCamp", true, new Action<Exception>(GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap), true);
            LongEventHandler.QueueLongEvent(delegate ()
            {
                Map map = camp.Map;
                Thing t = caravan.PawnsListForReading[0];
                CaravanEnterMapUtility.Enter(caravan, map, CaravanEnterMode.Center, CaravanDropInventoryMode.DoNotDrop, false, (IntVec3 x) => x.GetRoom(map).CellCount >= 600);
                CameraJumper.TryJump(t);
                Find.TickManager.CurTimeSpeed = 0;
                Messages.Message("SetUpCampFormedCamp".Translate(), MessageTypeDefOf.PositiveEvent);
            }, "SpawningColonists", true, new Action<Exception>(GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap), true);
        }

        public static CaravanCamp NewCaravanCamp(int tile, Faction faction)
        {
            CaravanCamp camp = (CaravanCamp)WorldObjectMaker.MakeWorldObject(SetUpCampDefOf.CaravanCamp);
            camp.Tile = tile;
            camp.SetFaction(faction);
            Find.WorldObjects.Add(camp);
            return camp;
        }

        //Generates a map with a defined seed
        private static Map GenerateNewMapWithSeed(int tile, IntVec3 size, string seed)
        {
            string cachedSeedString = Find.World.info.seedString;
            Find.World.info.seedString = seed;
            MapParent mapParent = Find.WorldObjects.MapParentAt(tile);
            Map generatedMap = MapGenerator.GenerateMap(size, mapParent, mapParent.MapGeneratorDef, mapParent.ExtraGenStepDefs, null);
            Find.World.info.seedString = cachedSeedString;
            return generatedMap;
        }
    }
}
