using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Qbeh1_SpeedrunMod.Common;
using Qbeh1_SpeedrunMod.Patches;
using System.Collections.Generic;
using UnityEngine;

namespace Qbeh1_SpeedrunMod
{
    [BepInPlugin(PluginConstants.PLUGIN_GUID, PluginConstants.PLUGIN_NAME, PluginConstants.PLUGIN_VERSION)]
    public class SpeedrunPlugin : BaseUnityPlugin
    {
        private const float PLAYER_EYE_HEIGHT = 1.665f;

        private static SpeedrunPlugin instance;

        private static bool showGUI;
        private static Rect guiRect = new Rect(10, 10, 400, 750);
        private static Vector2 inputScrollPos = Vector2.zero;
        private static Vector2 levelScrollPos = Vector2.zero;

        public static bool levelNamesInitialized;
        public static string[] levelNames;

        private static bool isNoclip;

        public static bool guiSkinInitialized;
        private static LabelRenderer leftRenderer;
        private static LabelRenderer rightRenderer;

        private static float lastInterval;
        private static float fps;
        private static int frames;

        private void Awake()
        {
            Logger.LogInfo($"{PluginConstants.PLUGIN_NAME} (v{PluginConstants.PLUGIN_VERSION}) by KabanFriends");

            instance = this;

            // Load PlayerPrefs
            SpeedrunSettings.LoadSettings();

            // Patch classes
            Harmony.CreateAndPatchAll(typeof(SetupInputPatch));
            Harmony.CreateAndPatchAll(typeof(CameraFadePatch));
            Harmony.CreateAndPatchAll(typeof(GameStatePatch));
            Harmony.CreateAndPatchAll(typeof(MainMenuBehaviourPatch));
            Harmony.CreateAndPatchAll(typeof(LoadLevelPatch));
            Harmony.CreateAndPatchAll(typeof(LevelStartLogicPatch));
            Harmony.CreateAndPatchAll(typeof(HUDFpsPatch));
            Harmony.CreateAndPatchAll(typeof(SetupKeyBindingsPatch));

            // Get necessary assets from other levels
            Application.LoadLevel("level_load"); // Level names
            Application.LoadLevel("level_1-1"); // GUI skin

            // Continue intro sequence
            if (SpeedrunSettings.skipIntro)
            {
                Application.LoadLevel("menu_main");
            } else
            {
                Application.LoadLevel("intro_sequence");
            }

            // Set up LabelRenderers
            leftRenderer = new LabelRenderer(0f, 0f);
            leftRenderer.alignment = TextAnchor.UpperLeft;

            rightRenderer = new LabelRenderer(Screen.width - 100f, 0f);
            rightRenderer.alignment = TextAnchor.UpperLeft;

            // Set up FPS Counter
            lastInterval = Time.realtimeSinceStartup;
            frames = 0;
        }

        private void OnDestroy()
        {
            SpeedrunSettings.SaveSettings();
        }

        private void Update()
        {
            // Inputs
            if (Event.current != null)
            {
                if (Event.current.isKey && Event.current.keyCode == KeyCode.F10 && Event.current.type == EventType.KeyDown)
                {
                    showGUI = !showGUI;
                }
            }
            

            // Low gravity
            if (SpeedrunSettings.forceLowGrav)
            {
                if (PlayerUtils.IsPlayerMotorEnabled())
                {
                    PlayerLogic logic = GameObject.Find("char_player").GetComponent<PlayerLogic>();
                    Traverse t = Traverse.Create(logic);
                    CharacterMotor motor = t.Field("motor").GetValue<CharacterMotor>();

                    motor.jumping.baseHeight = motor.jumping.lowGravityHeight;
                    motor.jumping.extraHeight = motor.jumping.lowGravityExtraHeight;
                    motor.movement.gravity = motor.movement.lowGravity;

                    if (t.Field("gravityFieldCount").GetValue<int>() == 0)
                    {
                        t.Field("gravityFieldCount").SetValue(1);
                    }

                    t.Field("lowGravity").SetValue(true);
                }
            }

            // Noclip
            if (cInput.GetButtonDown("Speedrun Noclip"))
            {
                if (PlayerUtils.IsPlayerMotorEnabled())
                {
                    isNoclip = !isNoclip;

                    PlayerLogic logic = GameObject.Find("char_player").GetComponent<PlayerLogic>();
                    Traverse t = Traverse.Create(logic);
                    CharacterMotor motor = t.Field("motor").GetValue<CharacterMotor>();

                    if (isNoclip)
                    {
                        t.Field("freefalling").SetValue(false);
                        t.Field("windFallAS").GetValue<AudioSource>().Stop();

                        motor.ignoreGravityStuff = true;
                        motor.canControl = false;
                        motor.SetVelocity(new Vector3());

                        // Not sure why this works... Better method is appriciated
                        ((CharacterController)logic.collider).center = new Vector3(50000, 50000, 50000);
                    } else
                    {
                        motor.ignoreGravityStuff = false;
                        motor.canControl = true;
                        ((CharacterController)logic.collider).center = new Vector3();
                    }
                }
            }

            if (isNoclip)
            {
                if (GameObject.Find("char_player"))
                {
                    PlayerLogic logic = GameObject.Find("char_player").GetComponent<PlayerLogic>();
                    Traverse t = Traverse.Create(logic);
                    CharacterMotor motor = t.Field("motor").GetValue<CharacterMotor>();

                    var speed = 10;

                    if (cInput.GetButton("Run"))
                    {
                        speed = 25;
                    }

                    motor.transform.position += Camera.main.transform.forward * Time.deltaTime * cInput.GetAxis("Vertical") * speed;
                    motor.transform.position += Camera.main.transform.right * Time.deltaTime * cInput.GetAxis("Horizontal") * speed;
                } else
                {
                    isNoclip = false;
                }
            }

            // FPS counter
            frames++;
            float sinceStartup = Time.realtimeSinceStartup;
            if (sinceStartup > lastInterval + 0.5f)
            {
                fps = frames / (sinceStartup - lastInterval);
                frames = 0;
                lastInterval = sinceStartup;
            }

            // Left labels
            leftRenderer.labels.Clear();

            leftRenderer.labels.Add($"Speedrun Mod v{PluginConstants.PLUGIN_VERSION} - F10 for menu");
            leftRenderer.labels.Add(fps.ToString("f2") + " FPS");
            leftRenderer.labels.Add("");

            List<string> status = new List<string>();
            if (isNoclip)
            {
                status.Add("Noclip");
            }
            if (SpeedrunSettings.forceLowGrav)
            {
                status.Add("LowGrav");
            }

            leftRenderer.labels.Add(string.Join(", ", status.ToArray()));

            // Right labels
            rightRenderer.labels.Clear();

            if (SpeedrunSettings.playerPosition)
            {
                rightRenderer.labels.Add("Position:");

                if (PlayerUtils.IsPlayerExists())
                {
                    PlayerLogic logic = GameObject.Find("char_player").GetComponent<PlayerLogic>();
                    Vector3 vec = logic.transform.position;

                    rightRenderer.labels.Add("X: " + vec.x.ToString("0.00"));
                    rightRenderer.labels.Add("Y: " + (vec.y - PLAYER_EYE_HEIGHT).ToString("0.00"));
                    rightRenderer.labels.Add("Z: " + vec.z.ToString("0.00"));
                }
                else
                {
                    rightRenderer.labels.Add("None");
                }
            }

            if (SpeedrunSettings.playerRotation)
            {
                if (rightRenderer.labels.Count > 0)
                {
                    rightRenderer.labels.Add("");
                }

                rightRenderer.labels.Add("Rotation:");

                if (PlayerUtils.IsPlayerExists())
                {
                    Vector3 vec = Camera.main.transform.localEulerAngles;

                    var x = vec.x;
                    if (x > 180)
                    {
                        x -= 360;
                    }

                    var y = vec.y;
                    if (y > 180)
                    {
                        y -= 360;
                    }

                    rightRenderer.labels.Add("Pitch: " + x.ToString("0.00"));
                    rightRenderer.labels.Add("Yaw: " + y.ToString("0.00"));
                }
                else
                {
                    rightRenderer.labels.Add("None");
                }
            }

            if (SpeedrunSettings.playerVelocity)
            {
                if (rightRenderer.labels.Count > 0)
                {
                    rightRenderer.labels.Add("");
                }

                rightRenderer.labels.Add("Velocity:");

                if (PlayerUtils.IsPlayerExists())
                {
                    PlayerLogic logic = GameObject.Find("char_player").GetComponent<PlayerLogic>();
                    Traverse t = Traverse.Create(logic);
                    CharacterMotor motor = t.Field("motor").GetValue<CharacterMotor>();

                    Vector3 vec = motor.movement.velocity;

                    rightRenderer.labels.Add("X: " + vec.x.ToString("0.00"));
                    rightRenderer.labels.Add("Y: " + vec.y.ToString("0.00"));
                    rightRenderer.labels.Add("Z: " + vec.z.ToString("0.00"));
                }
                else
                {
                    rightRenderer.labels.Add("None");
                }
            }
        }

        private void OnGUI()
        {
            if (showGUI)
            {
                guiRect = GUI.ModalWindow(10000, guiRect, new GUI.WindowFunction(DoWindow), "Speedrun Mod");
            }

            leftRenderer.Draw();
            rightRenderer.Draw();
        }

        private void OnLevelWasLoaded(int index)
        {
            if (index == 7) //level_1-1
            {
                if (!guiSkinInitialized)
                {
                    if (FindObjectOfType<HUDFps>())
                    {
                        SetupGUISkin(FindObjectOfType<HUDFps>().skin);
                    }
                }
            }
        }

        private void DoWindow(int windowId)
        {
            // Tweaks menu
            bool prevLowGravity = SpeedrunSettings.forceLowGrav;

            GUILayout.Label("Tweaks:");

            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            SpeedrunSettings.noFade = GUILayout.Toggle(SpeedrunSettings.noFade, "Disable fade in/out");
            SpeedrunSettings.forceLowGrav = GUILayout.Toggle(SpeedrunSettings.forceLowGrav, "Force low gravity");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            SpeedrunSettings.skipIntro = GUILayout.Toggle(SpeedrunSettings.skipIntro, "Skip logo intro");
            GUILayout.EndHorizontal();

            if (prevLowGravity != SpeedrunSettings.forceLowGrav)
            {
                if (GameObject.Find("char_player"))
                {
                    PlayerLogic logic = GameObject.Find("char_player").GetComponent<PlayerLogic>();
                    Traverse t = Traverse.Create(logic);
                    CharacterMotor motor = t.Field("motor").GetValue<CharacterMotor>();
                    int gravityFieldCount = t.Field("gravityFieldCount").GetValue<int>();

                    if (SpeedrunSettings.forceLowGrav == true)
                    {
                        if (gravityFieldCount == 0)
                        {
                            float y = motor.CalculateJumpVerticalSpeed(motor.jumping.lowGravityHeight - (logic.transform.position.y - t.Field("lastSafePosition").GetValue<Vector3>().y));
                            motor.movement.velocity.y = y;
                        }

                        t.Field("gravityFieldCount").SetValue(gravityFieldCount + 1);
                    }
                    else
                    {
                        if (gravityFieldCount == 1)
                        {
                            motor.jumping.baseHeight = motor.jumping.normalBaseHeight;
                            motor.jumping.extraHeight = motor.jumping.normalExtraHeight;
                            motor.movement.gravity = motor.movement.normalGravity;

                            t.Field("lowGravity").SetValue(false);
                        }
                        if (gravityFieldCount > 0)
                        {
                            t.Field("gravityFieldCount").SetValue(gravityFieldCount - 1);
                        }
                    }
                }
            }

            GUILayout.FlexibleSpace();

            // Display menu
            GUILayout.Label("Display:");

            /*
            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            SpeedrunSettings.levelTimer = GUILayout.Toggle(SpeedrunSettings.levelTimer, "IL timer");
            SpeedrunSettings.levelSumTimer = GUILayout.Toggle(SpeedrunSettings.levelSumTimer, "IL sum timer");
            SpeedrunSettings.allTimer = GUILayout.Toggle(SpeedrunSettings.allTimer, "Full timer");
            GUILayout.EndHorizontal();
            */

            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            SpeedrunSettings.playerPosition = GUILayout.Toggle(SpeedrunSettings.playerPosition, "Coordinates");
            SpeedrunSettings.playerRotation = GUILayout.Toggle(SpeedrunSettings.playerRotation, "Rotation");
            SpeedrunSettings.playerVelocity = GUILayout.Toggle(SpeedrunSettings.playerVelocity, "Velocity");
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            // Input menu
            GUILayout.Label("Input:");
            inputScrollPos = GUILayout.BeginScrollView(inputScrollPos, new GUILayoutOption[0]);

            for (int i = 0; i < cInput.length; i++)
            {
                string text = cInput.GetText(i, 0);
                if (text.StartsWith("Speedrun "))
                {
                    GUILayout.BeginHorizontal(new GUILayoutOption[0]);
                    GUILayout.Label(text.Substring(9), new GUILayoutOption[0]);
                    if (GUILayout.Button(cInput.GetText(i, 1), new GUILayoutOption[]
                    {
                            GUILayout.Width(150f)
                    }))
                    {
                        cInput.ChangeKey(i, 1, false, false, false, false, true);
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.Space(20f);

            if (GUILayout.Button("Reset Inputs to Defaults", new GUILayoutOption[0]))
            {
                SpeedrunSettings.ResetCInputKeys();
            }
            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            // Level select menu
            GUILayout.Label("Levels:");

            levelScrollPos = GUILayout.BeginScrollView(levelScrollPos, new GUILayoutOption[]
            {
                GUILayout.Height(300f)
            });

            for (int world = 1; world <= 6; world++)
            {
                for (int level = 1; level <= 6; level++)
                {
                    if (levelNamesInitialized)
                    {
                        GUILayout.BeginHorizontal(new GUILayoutOption[0]);
                        if (GUILayout.Button($"{world}-{level}: {levelNames[(world - 1) * 6 + (level - 1)]}", new GUILayoutOption[0]))
                        {
                            if (GameObject.Find("game_state"))
                            {
                                GameState gameState = GameObject.Find("game_state").GetComponent<GameState>();
                                gameState.levelToLoad = $"level_{world}-{level}";

                                Application.LoadLevel("level_load");
                            }
                            else
                            {
                                Log(LogLevel.Error, "game_state was not found, cannot start level!");
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            }

            GUILayout.EndScrollView();
        }

        public static void SetupLevelNames(List<string> list)
        {
            levelNames = list.ToArray();
            levelNamesInitialized = true;
        }

        public static void SetupGUISkin(GUISkin skin)
        {
            leftRenderer.skin = skin;
            rightRenderer.skin = skin;

            guiSkinInitialized = true;
        }

        public static void Log(string message)
            => Log(LogLevel.Info, message);

        public static void Log(LogLevel level, string message)
        {
            instance.Logger.Log(level, message);
        }
    }
}