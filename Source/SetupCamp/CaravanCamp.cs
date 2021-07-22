using HarmonyLib;
using System;
using System.Data;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Syrchalis_SetUpCamp
{
    public class CaravanCamp : MapParent
    {
        public bool startedCountdown;

        public override MapGeneratorDef MapGeneratorDef => SetUpCampSettings.customMapGenDef ? SetUpCampDefOf.Syr_SetUpCamp : SetUpCampDefOf.Syr_SetUpCampNR;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.startedCountdown, "startedCountdown", false, false);
        }

        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            if (!Map.mapPawns.AnyPawnBlockingMapRemoval && !SetUpCampSettings.permanentCamps)
            {
                if (SetUpCampSettings.timeout != SetUpCampSettings.timeoutmin) //this would mean the setting is off
                {
                    AddAbandonedCamp(this);
                }
                alsoRemoveWorldObject = true;
                return true;
            }
            alsoRemoveWorldObject = false;
            return false;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            if (HasMap && SetUpCampSettings.permanentCamps)
            {
                yield return new Command_Action
                {
                    defaultLabel = "SetUpCampDestroyCamp".Translate(),
                    defaultDesc = "SetUpCampDestroyCampDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/DeleteCamp", true),
                    action = delegate ()
                    {
                        Find.WindowStack.Add(new Dialog_MessageBox("SetUpCampAbandonDialogue".Translate(),
                            "AcceptButton".Translate(), delegate ()
                            {
                                Messages.Message("SetUpCampAbandoned".Translate(), MessageTypeDefOf.TaskCompletion);
                                
                                if (SetUpCampSettings.timeout != SetUpCampSettings.timeoutmin) //this would mean the setting is off
                                {
                                    AddAbandonedCamp(this);
                                }
                            },
                            "CancelButton".Translate(), null, null, false, null, null));
                    }
                };
            }
        }
        
        public void AddAbandonedCamp(CaravanCamp Camp)
        {
            WorldObject worldObject = WorldObjectMaker.MakeWorldObject(SetUpCampDefOf.AbandonedCamp);
            worldObject.GetComponent<TimeoutComp>().StartTimeout(SetUpCampSettings.timeout * 60000);
            worldObject.Tile = Camp.Tile;
            worldObject.SetFaction(Camp.Faction);
            Find.WorldObjects.Add(worldObject);
        }

    }
}
