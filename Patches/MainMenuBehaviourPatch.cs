using HarmonyLib;
using Qbeh1_SpeedrunMod.Common;
using System.Collections;
using UnityEngine;

namespace Qbeh1_SpeedrunMod.Patches
{
    public class MainMenuBehaviourPatch
    {
        [HarmonyPatch(typeof(MainMenuBehaviour), "FadeOutToLoadLevel")]
        [HarmonyPostfix]
        private static IEnumerator FadeOutPatch(IEnumerator result, string levelName)
        {
            if (SpeedrunSettings.noFade)
            {
                Screen.lockCursor = true;
                Screen.showCursor = false;
                Application.LoadLevel(levelName);

                yield break;
            }

            while (result.MoveNext())
            {
                yield return result.Current;
            }
        }
    }
}
