using HarmonyLib;
using Qbeh1_SpeedrunMod.Common;
using System.Collections;

namespace Qbeh1_SpeedrunMod.Patches
{
    internal class GameStatePatch
    {
        [HarmonyPatch(typeof(GameState), "Update")]
        [HarmonyPrefix]
        private static void UpdatePatch(GameState __instance)
        {
            if (SpeedrunSettings.noFade)
            {
                __instance.camFadeOutLength = 0f;
                __instance.camFadeInLength = 0f;
            }
            else
            {
                __instance.camFadeOutLength = 3f;
                __instance.camFadeInLength = 3f;
            }
        }

        [HarmonyPatch(typeof(GameState), "FadeOutAudio")]
        [HarmonyPostfix]
        private static IEnumerator FadeOutAudioPatch(IEnumerator result)
        {
            if (SpeedrunSettings.noFade)
            {
                yield break;
            }

            while (result.MoveNext())
            {
                yield return result.Current;
            }
        }
    }
}
