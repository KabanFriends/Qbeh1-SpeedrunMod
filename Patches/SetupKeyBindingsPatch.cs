using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace Qbeh1_SpeedrunMod.Patches
{
    public class SetupKeyBindingsPatch
    {
        [HarmonyPatch(typeof(SetupKeyBindings), "Start")]
        [HarmonyPrefix]
        private static bool StartPatch(SetupKeyBindings __instance)
        {
            for (int i = 0; i < cInput.length; i++)
            {
                if (!cInput.GetText(i).Contains("Mouse Look") && !cInput.GetText(i).StartsWith("Editor ") && !cInput.GetText(i).StartsWith("Speedrun "))
                {
                    GameObject gameObject = NGUITools.AddChild(__instance.gameObject, __instance.controlPrefab);
                    int depth = gameObject.GetComponent<UIWidget>().depth;
                    Transform transform = gameObject.transform.FindChild("Label");
                    string text;
                    if (cInput.GetText(i) == "Run")
                    {
                        text = "Run/Walk";
                    }
                    else
                    {
                        text = cInput.GetText(i);
                    }
                    transform.GetComponent<UILabel>().text = text;
                    transform = gameObject.transform.FindChild("Primary Binding");
                    transform.GetComponent<UIWidget>().depth = depth;
                    transform.FindChild("Label").GetComponent<UIWidget>().depth = depth;
                    ChangeKeyBinding component = transform.GetComponent<ChangeKeyBinding>();
                    component.bindingName = cInput.GetText(i);
                    component.index = 1;
                    transform = gameObject.transform.FindChild("Secondary Binding");
                    transform.GetComponent<UIWidget>().depth = depth;
                    transform.FindChild("Label").GetComponent<UIWidget>().depth = depth;
                    component = transform.GetComponent<ChangeKeyBinding>();
                    component.bindingName = cInput.GetText(i);
                    component.index = 2;
                }
            }

            __instance.StartCoroutine(Traverse.Create(__instance).Method("UpdateKeyBindingsGrid").GetValue<IEnumerator>());

            return false;
        }
    }
}
