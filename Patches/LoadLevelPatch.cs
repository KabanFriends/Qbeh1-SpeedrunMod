using HarmonyLib;
using Qbeh1_SpeedrunMod.Common;
using System.Collections.Generic;
using UnityEngine;

namespace Qbeh1_SpeedrunMod.Patches
{
    public class LoadLevelPatch
    {
        [HarmonyPatch(typeof(LoadLevel), "Awake")]
        [HarmonyPrefix]
        private static bool AwakePatch(LoadLevel __instance)
        {
            if (!SpeedrunPlugin.levelNamesInitialized)
            {
                SpeedrunPlugin.SetupLevelNames((List<string>)Traverse.Create(__instance).Field("levelNames").GetValue());
                return false;
            }

            if (SpeedrunSettings.noFade)
            {
                Traverse t = Traverse.Create(__instance);

                t.Field("fadeInDuration").SetValue(0f);
                t.Field("fadeOutDuration").SetValue(0f);
            }
            return true;
        }

        [HarmonyPatch(typeof(LoadLevel), "Start")]
        [HarmonyPostfix]
        private static void StartPatch(LoadLevel __instance)
        {
            if (SpeedrunSettings.noFade)
            {
                AudioListener.volume = 1;
                ((UILabel)Traverse.Create(__instance).Field("levelNameLabel").GetValue()).alpha = 1f;
            }
        }
    }
}
