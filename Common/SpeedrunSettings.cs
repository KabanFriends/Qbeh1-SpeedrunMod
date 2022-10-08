using System;
using System.Reflection;
using UnityEngine;

namespace Qbeh1_SpeedrunMod.Common
{
    public class SpeedrunSettings
    {
        // Tweaks
        public static bool noFade;
        public static bool forceLowGrav;
        public static bool skipIntro;

        // Display
        public static bool levelTimer = true;
        public static bool levelSumTimer;
        public static bool allTimer;
        public static bool playerPosition;
        public static bool playerVelocity;
        public static bool playerRotation;

        public static void LoadSettings()
        {
            foreach (FieldInfo field in typeof(SpeedrunSettings).GetFields())
            {
                string prefKey = "speedrun_" + field.Name;

                if (PlayerPrefs.HasKey(prefKey))
                {
                    switch (Type.GetTypeCode(field.FieldType))
                    {
                        case TypeCode.Boolean:
                            field.SetValue(null, Convert.ToBoolean(PlayerPrefs.GetInt(prefKey)));
                            break;
                        case TypeCode.Int32:
                            field.SetValue(null, PlayerPrefs.GetInt(prefKey));
                            break;
                        case TypeCode.String:
                            field.SetValue(null, PlayerPrefs.GetString(prefKey));
                            break;
                    }
                }
            }
        }

        public static void SaveSettings()
        {
            foreach (FieldInfo field in typeof(SpeedrunSettings).GetFields())
            {
                string prefKey = "speedrun_" + field.Name;

                switch (Type.GetTypeCode(field.FieldType))
                {
                    case TypeCode.Boolean:
                        PlayerPrefs.SetInt(prefKey, Convert.ToInt32(field.GetValue(null)));
                        break;
                    case TypeCode.Int32:
                        PlayerPrefs.SetInt(prefKey, (int)field.GetValue(null));
                        break;
                    case TypeCode.String:
                        PlayerPrefs.SetString(prefKey, (string)field.GetValue(null));
                        break;
                }
            }
        }

        public static void SetCInputKeys()
        {
            //cInput.SetKey("Speedrun Retry from Start", "F1");
            //cInput.SetKey("Speedrun Retry from Checkpoint", "F2");
            //cInput.SetKey("Speedrun Timer Pause/Resume", "F3");
            //cInput.SetKey("Speedrun Reset Timer", "F4");
            //cInput.SetKey("Speedrun Set Checkpoint", "F5");
            cInput.SetKey("Speedrun Noclip", "F6");
        }

        public static void ResetCInputKeys()
        {
            //cInput.ChangeKey("Speedrun Retry from Start", "F1");
            //cInput.ChangeKey("Speedrun Retry from Checkpoint", "F2");
            //cInput.ChangeKey("Speedrun Timer Pause/Resume", "F3");
            //cInput.ChangeKey("Speedrun Reset Timer", "F4");
            //cInput.ChangeKey("Speedrun Set Checkpoint", "F5");
            cInput.ChangeKey("Speedrun Noclip", "F6");
        }
    }
}
