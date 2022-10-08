using HarmonyLib;
using Qbeh1_SpeedrunMod.Common;
using UnityEngine;

namespace Qbeh1_SpeedrunMod.Patches
{
    internal class CameraFadePatch
    {
        [HarmonyPatch(typeof(CameraFade), "StartFade")]
        [HarmonyPrefix]
        private static bool StartFadePatch(CameraFade __instance)
        {
            if (SpeedrunSettings.noFade) {
                __instance.SetScreenOverlayColor(Color.clear);
                return false;
            }
            return true;
        }
    }
}
