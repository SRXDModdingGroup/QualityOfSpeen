using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MewsToolbox;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace QualityOfSpeen
{
    [BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
    public class Main : BaseUnityPlugin
    {
        #region Mod Metadata
        public const string MOD_ID = "QualityOfSpeen";
        public const string MOD_NAME = "Quality of Speen";
        public const string MOD_VERSION = "1.0.2";
        #endregion

        #region Mod Variables
        private static ManualLogSource logger;
        #endregion

        #region Plugin Awake
        void Awake()
        {
            logger = Logger;
            Harmony harmony = new Harmony(MOD_ID);

            // Sets Global Game Variables to use by other classes
            harmony.PatchAll<StatePatches>();

            // Load config. If it does not exist, create one.
            if (!File.Exists(configFilePath))
                File.Create(configFilePath).Close();
            modConfig = new IniFile(configFilePath);

            // Autoload all Quality of Speen Features in the Features namespace
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes().Where(t=>string.Equals(t.Namespace, "QualityOfSpeen.Features", StringComparison.Ordinal))) // Source: https://stackoverflow.com/questions/949246/how-can-i-get-all-classes-within-a-namespace
            {
                LogInfo($"QoS: Loading feature {type.ToString().Replace(type.Namespace+".", "")}");
                try
                {
                    harmony.PatchAll(type);
                }
                catch (Exception e)
                {
                    LogError($"Failed to load {type}: {e}");
                }
            }
        }
        #endregion

        #region Logger

        public static void Log(LogLevel level, object msg) => logger.Log(level, msg);
        public static void LogInfo(object msg) => Log(LogLevel.Info, msg);
        public static void LogWarning(object msg) => Log(LogLevel.Warning, msg);
        public static void LogError(object msg) => Log(LogLevel.Error, msg);
        public static void LogDebug(object msg) => Log(LogLevel.Debug, msg);
        public static void LogMessage(object msg) => Log(LogLevel.Message, msg);

        #endregion

        #region Mod Config
        private static IniFile modConfig;
        public static IniFile ModConfig => modConfig;
        private static string configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Speen Mods", "QualityOfSpeenConfig.ini");
        #endregion

        #region Global Game Variables
        private static InGameState inGameState;
        public static InGameState InGameState { get => inGameState; private set { LogMessage("Changed ingamestate: " + value); inGameState = value; } }
        public static bool IsPlayingCustom { get; private set; }
        public static bool IsInCustomsMenu { get; private set; }
        public static bool IsRestarting { get; private set; }
        public static bool IsDead { get; private set; }
        public static bool HasWon { get; private set; }
        #endregion

        #region Game Variables Setters
        public class StatePatches
        {
            private static void ResetTrackVariables()
            {
                IsRestarting = false;
                IsDead = false;
                HasWon = false;
            }

            [HarmonyPatch(typeof(Track), nameof(Track.CompleteSong))]
            [HarmonyPrefix]
            private static void CompleteSongPrefix()
            {
                HasWon = true;
            }

            [HarmonyPatch(typeof(Track), nameof(Track.FailSong))]
            [HarmonyPrefix]
            private static void FailSongPrefix()
            {
                IsDead = true;
            }

            [HarmonyPatch(typeof(Track), nameof(Track.PlayTrack))]
            [HarmonyPrefix]
            private static void PlayTrackPrefix()
            {
                if (Track.IsEditing && InGameState != InGameState.Editor) InGameState = InGameState.Editor;
                if (InGameState == InGameState.Editor) return;
                if (InGameState == InGameState.CustomLevelSelectMenu) IsPlayingCustom = true;
                InGameState = InGameState.Playing;
                ResetTrackVariables();
            }

            [HarmonyPatch(typeof(Track), nameof(Track.RestartTrack))]
            [HarmonyPrefix]
            private static void RestartTrackPrefix()
            {
                IsRestarting = true;
            }

            [HarmonyPatch(typeof(XDMainMenu), nameof(XDMainMenu.OpenMenu))]
            [HarmonyPrefix]
            private static void XDMainMenuOpenPrefix()
            {
                IsInCustomsMenu = false;
                InGameState = InGameState.MainMenu;
            }

            [HarmonyPatch(typeof(XDOptionsMenu), nameof(XDOptionsMenu.OpenMenu))]
            [HarmonyPrefix]
            private static void XDOptionsMenuOpenPrefix()
            {
                InGameState = InGameState.OptionsMenu;
            }

            [HarmonyPatch(typeof(XDCustomLevelSelectMenu), nameof(XDCustomLevelSelectMenu.OpenMenu))]
            [HarmonyPrefix]
            private static void XDCustomLevelSelectMenuOpenPrefix()
            {
                InGameState = InGameState.CustomLevelSelectMenu;
                IsPlayingCustom = false;
                IsInCustomsMenu = true;
                ResetTrackVariables();
            }

            [HarmonyPatch(typeof(XDLevelSelectMenuBase), nameof(XDLevelSelectMenuBase.OpenMenu))]
            [HarmonyPrefix]
            private static void XDLevelSelectMenuBaseOpenPrefix()
            {
                if (IsInCustomsMenu) return;
                InGameState = InGameState.LevelSelectMenu;
                ResetTrackVariables();
            }
        }
        #endregion
    }
}
