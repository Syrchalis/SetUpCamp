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
                }
            }
            else
            {
                result = false;
                alsoRemoveWorldObject = false;
            }
            return result;
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

        public bool shouldBeDeleted = false;
    }
}
