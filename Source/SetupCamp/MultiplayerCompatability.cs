using Multiplayer.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Syrchalis_SetUpCamp
{
    class MultiplayerCompatability
    {
        [StaticConstructorOnStartup]
        static class MultiplayerCompatibility
        {
            static MultiplayerCompatibility()
            {
                if (!MP.enabled) return;

                MP.RegisterSyncMethod(typeof(SetUpCamp_Utility), nameof(SetUpCamp_Utility.Camp));

            }
        }
    }
}
