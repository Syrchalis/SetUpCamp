using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Syrchalis_SetUpCamp
{
    public class CaravanCamp : MapParent
    {
        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void Tick()
        {
            base.Tick();
            if (HasMap)
            {
                CheckStartForceExitAndRemoveMapCountdown();
            }
        }

        private bool startedCountdown;
        public float forceExitAndRemoveMapCountdownDurationDays = SetUpCampSettings.mapTimerDays;
        private void CheckStartForceExitAndRemoveMapCountdown()
        {
            if (!startedCountdown && SetUpCampSettings.mapTimerDays != SetUpCampSettings.mapTimerDaysmin && SetUpCampSettings.mapTimerDays != SetUpCampSettings.mapTimerDaysmax)
            {
                if (!GenHostility.AnyHostileActiveThreatToPlayer(Map))
                {
                    startedCountdown = true;
                    int num = Mathf.RoundToInt(forceExitAndRemoveMapCountdownDurationDays * 60000f);
                    string text = "MessageSiteCountdownBecauseNoEnemiesInitially".Translate(new object[]
                    {
                        TimedForcedExit.GetForceExitAndRemoveMapCountdownTimeLeftString(num)
                    });
                    Messages.Message(text, this, MessageTypeDefOf.PositiveEvent, true);
                    GetComponent<TimedForcedExit>().StartForceExitAndRemoveMapCountdown(num);
                }
            }
        }
        public bool shouldBeDeleted = false;
        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            bool result;
            bool pawnblock = Map.mapPawns.AnyPawnBlockingMapRemoval;
            if (!pawnblock)
            {
                bool permCamps = SetUpCampSettings.permanentCamps && !shouldBeDeleted;
                if (permCamps)
                {
                    alsoRemoveWorldObject = false;
                    result = false;
                }
                else
                {
                    alsoRemoveWorldObject = true;
                    result = true;
                    if (SetUpCampSettings.timeout != SetUpCampSettings.timeoutmin && SetUpCampSettings.timeout != SetUpCampSettings.timeoutmax)
                    {
                        AddAbandonedCamp(this);
                    }     
                }
            }
            else
            {
                result = false;
                alsoRemoveWorldObject = false;
            }
            return result;
        }

        private static void AddAbandonedCamp(CaravanCamp Camp)
        {
            WorldObject worldObject = WorldObjectMaker.MakeWorldObject(SetUpCampDefOf.AbandonedCamp);
            worldObject.GetComponent<TimeoutComp>().StartTimeout(SetUpCampSettings.timeout * 60000);
            worldObject.Tile = Camp.Tile;
            worldObject.SetFaction(Camp.Faction);
            Find.WorldObjects.Add(worldObject);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            SetUpCampSettings settings = LoadedModManager.GetMod<SetUpCamp>().GetSettings<SetUpCampSettings>();
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
                g.disabledReason = null;
            }
            bool showDestroyButton = HasMap && SetUpCampSettings.permanentCamps;
            if (showDestroyButton)
            {
                yield return new Command_Action
                {
                    defaultLabel = "SetUpCampDestroyCamp".Translate(),
                    defaultDesc = "SetUpCampDestroyCampDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/DeleteCamp", true),
                    action = delegate ()
                    {
                        bool pawnblock = Map.mapPawns.AnyPawnBlockingMapRemoval;
                        if (!pawnblock)
                        {
                            Find.WindowStack.Add(new Dialog_MessageBox("SetUpCampAbandonDialogue".Translate(),
                                "AcceptButton".Translate(), delegate ()
                                {
                                    Messages.Message("SetUpCampAbandoned".Translate(), MessageTypeDefOf.TaskCompletion);
                                    shouldBeDeleted = true;
                                    Tick();
                                },
                                "CancelButton".Translate(), null, null, false, null, null));
                        }
                        else
                        {
                            Messages.Message("SetUpCampStillInUse".Translate(), MessageTypeDefOf.RejectInput);
                            shouldBeDeleted = false;
                        }
                    }
                };
            }
            yield break;
        }
    }
}
