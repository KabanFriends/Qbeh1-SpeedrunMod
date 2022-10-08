using HarmonyLib;
using Qbeh1_SpeedrunMod.Common;
using System.Collections;
using UnityEngine;

namespace Qbeh1_SpeedrunMod.Patches
{
    public class LevelStartLogicPatch
    {
        [HarmonyPatch(typeof(LevelStartLogic), "Start")]
        [HarmonyPrefix]
        private static void StartPatch(LevelStartLogic __instance)
        {
            if (SpeedrunSettings.noFade)
            {
                Traverse t = Traverse.Create(__instance);

                t.Field("cameraFadeInStartDelay").SetValue(0f);
                t.Field("cameraFadeInLength").SetValue(0f);
                t.Field("cameraFadeOutLength").SetValue(0f);
            }
        }

        [HarmonyPatch(typeof(LevelStartLogic), "FadeInAudio")]
        [HarmonyPostfix]
        private static IEnumerator FadeInAudioPatch(IEnumerator result)
        {
            if (SpeedrunSettings.noFade)
            {
                AudioListener.volume = 1;
                yield break;
            }

            while (result.MoveNext())
            {
                yield return result.Current;
            }
        }

        [HarmonyPatch(typeof(LevelStartLogic), "FadeInLevelName")]
        [HarmonyPostfix]
        private static IEnumerator FadeInLevelNamePatch(IEnumerator result, LevelStartLogic __instance)
        {
            if (SpeedrunSettings.noFade)
            {
                Color temp = __instance.communityLevelSkin.label.normal.textColor;
                temp.a = 1;
                __instance.communityLevelSkin.label.normal.textColor = temp;

                yield break;
            }

            while (result.MoveNext())
            {
                yield return result.Current;
            }
        }

        [HarmonyPatch(typeof(LevelStartLogic), "FadeOutLevelName")]
        [HarmonyPostfix]
        private static IEnumerator FadeOutLevelNamePatch(IEnumerator result, LevelStartLogic __instance)
        {
            if (SpeedrunSettings.noFade)
            {
                Color temp = __instance.communityLevelSkin.label.normal.textColor;
                temp.a = 0;
                __instance.communityLevelSkin.label.normal.textColor = temp;

                yield break;
            }

            while (result.MoveNext())
            {
                yield return result.Current;
            }
        }
    }
}
