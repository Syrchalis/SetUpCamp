using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace SetUpCamp
{
    // Token: 0x02000002 RID: 2
    public class CaravanCamp : MapParent
    {
        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        public override void ExposeData()
        {
            base.ExposeData();
        }

        // Token: 0x06000002 RID: 2 RVA: 0x0000205C File Offset: 0x0000025C
        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            bool flag = !base.Map.mapPawns.AnyPawnBlockingMapRemoval;
            bool result;
            if (flag)
            {
                bool flag2 = SetUpCampSettings.permanentCamps && !this.shouldBeDeleted;
                if (flag2)
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
                alsoRemoveWorldObject = false;
                result = false;
            }
            return result;
        }

        // Token: 0x06000003 RID: 3 RVA: 0x000020B0 File Offset: 0x000002B0
        public override IEnumerable<Gizmo> GetGizmos()
        {
            SetUpCampSettings settings = LoadedModManager.GetMod<SetUpCamp>().GetSettings<SetUpCampSettings>();
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
                g.disabledReason = null;
            }
            IEnumerator<Gizmo> enumerator = null;
            bool flag = this.HasMap && SetUpCampSettings.permanentCamps;
            if (flag)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Destroy Camp",
                    defaultDesc = "Destroy the camp if there's no colonists, tame animals or prisoners on the map.",
                    icon = SetUpCampTextures.CampCommandTexDelete,
                    action = delegate ()
                    {
                        bool flag2 = !base.Map.mapPawns.AnyPawnBlockingMapRemoval;
                        if (flag2)
                        {
                            Find.WindowStack.Add(new Dialog_MessageBox("Are you sure you wish to destroy this camp? This is irreversible.", "Yes", delegate ()
                            {
                                Messages.Message("Camp destroyed.", MessageTypeDefOf.PositiveEvent);
                                this.shouldBeDeleted = true;
                            }, "No", delegate ()
                            {
                            }, "CONFIRM", false));
                        }
                        else
                        {
                            Messages.Message("Camp is still in use.", MessageTypeDefOf.RejectInput);
                            this.shouldBeDeleted = false;
                        }
                    }
                };
            }
            yield break;
        }

        // Token: 0x04000001 RID: 1
        public bool shouldBeDeleted = false;
    }
}
