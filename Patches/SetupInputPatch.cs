using HarmonyLib;
using Qbeh1_SpeedrunMod.Common;

namespace Qbeh1_SpeedrunMod.Patches
{
    public class SetupInputPatch
    {
        [HarmonyPatch(typeof(SetupInput), "SetupAllInputs")]
        [HarmonyPostfix]
        private static void AllInputsPatch()
        {
            SpeedrunSettings.SetCInputKeys();
        }

    }
}
