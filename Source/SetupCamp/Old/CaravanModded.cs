using System;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace SetUpCamp
{
    // Token: 0x02000004 RID: 4
    public class CaravanModded : Caravan
    {
        // Token: 0x0600000F RID: 15 RVA: 0x0000244F File Offset: 0x0000064F
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
                g.disabledReason = null;
            }
            IEnumerator<Gizmo> enumerator = null;
            bool flag = Find.WorldSelector.SingleSelectedObject == this;
            if (flag)
            {
                yield return CaravanCampUtility.CampCommand(this);
            }
            yield break;
        }

        // Token: 0x06000010 RID: 16 RVA: 0x0000245F File Offset: 0x0000065F
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref campMinimumSize, "campMinimumSize", 75, false);
            Scribe_Values.Look(ref campMaximumSize, "campMaximumSize", 90, false);
        }

        // Token: 0x0400000B RID: 11
        public int campMinimumSize = 75;

        // Token: 0x0400000C RID: 12
        public int campMaximumSize = 90;

        // Token: 0x0400000D RID: 13
        private List<Pawn> pawns = new List<Pawn>();
    }
}
