using UnityEngine;

namespace Qbeh1_SpeedrunMod.Common
{
    public class PlayerUtils
    {
        public static bool IsPlayerExists()
        {
            return GameObject.Find("char_player") != null;
        }

        public static bool IsPlayerMotorEnabled()
        {
            var motor = GameObject.Find("char_player")?.GetComponent<CharacterMotor>();
            
            if (motor != null)
            {
                return motor.enabled;
            }
            return false;
        }
    }
}
