using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;

namespace QualityOfSpeen
{
    [BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
    public class Main : BasePlugin
    {
        #region Mod Metadata
        public const string MOD_ID = "QualityOfSpeen";
        public const string MOD_NAME = "Quality of Speen";
        public const string MOD_VERSION = "1.0.0";
        #endregion

        #region Mod Variables
        public static BepInEx.Logging.ManualLogSource Logger;
        #endregion

        #region BasePlugin.Load() override
        public override void Load()
        {
            Logger = Log;
            Harmony harmony = new Harmony(MOD_ID);

            // Sets Global Game Variables to use by other classes
            harmony.PatchAll<StatePatches>();

            // Autoload all Quality of Speen Features in the Features namespace
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes().Where(t=>string.Equals(t.Namespace, "QualityOfSpeen.Features", StringComparison.Ordinal))) // Source: https://stackoverflow.com/questions/949246/how-can-i-get-all-classes-within-a-namespace
            {
                try
                {
                    harmony.PatchAll(type);
                    Log.LogInfo($"Loaded {type.ToString().Replace(type.Namespace+".", "")}");
                }
                catch (Exception e)
                {
                    Log.LogError($"Failed to load {type}: {e}");
                }
            }
        }
        #endregion

        #region Global Game Variables
        public static InGameState InGameState { get; private set; }
        public static bool IsPlayingCustom { get; private set; }
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
                ResetTrackVariables();
            }

            [HarmonyPatch(typeof(XDLevelSelectMenuBase), nameof(XDLevelSelectMenuBase.OpenMenu))]
            [HarmonyPrefix]
            private static void XDLevelSelectMenuBaseOpenPrefix()
            {
                InGameState = InGameState.LevelSelectMenu;
                ResetTrackVariables();
            }
        }
        #endregion
    }
}
