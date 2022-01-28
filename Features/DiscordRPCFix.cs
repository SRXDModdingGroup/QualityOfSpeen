using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace QualityOfSpeen.Features
{
    public class DiscordRPCFix
    {
        public static bool UpdateDiscord = Main.ModConfig.GetValueOrDefaultTo("Discord", "EnableRichPresence", true);
        public static bool SecretMode = Main.ModConfig.GetValueOrDefaultTo("Discord", "StartInSecretMode", false);

        [HarmonyPatch]
        class SpinDiscordPatch
        {
            static MethodBase TargetMethod()
            {
                var type = AccessTools.TypeByName("SpinDiscord");
                return AccessTools.Method(type, "UpdateActivityPresence");
            }

            static bool Prefix(ref string state, ref string details, ref string coverArt, ref string trackArtist, ref string trackTitle, ref string trackLabel, ref long endTime)
            {
                //Main.Logger.LogMessage("Activity Update");
                if (!UpdateDiscord) return false;


                if (Main.InGameState == InGameState.Editor)
                {
                    if (SecretMode)
                    {
                        details = "Editing <SECRET>";
                        state = "Secret Mode enabled!";
                        trackArtist = "Secret";
                        trackTitle = "Secret";
                        trackLabel = "Secret";
                        endTime = 0;
                    }
                    else
                    {
                        details = "Editing " + trackTitle;
                        endTime = 0;
                    }
                }

                if (Main.IsDead)
                {
                    state = "Failed";
                    endTime = 0;
                }

                if (Main.InGameState == InGameState.CustomLevelSelectMenu)
                {
                    state = "Picking Custom Track";
                    endTime = 0;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Track), "UpdateRichPresence")]
        [HarmonyPrefix]
        private static bool TrackUpdateRichPresencePrefix()
        {
            return UpdateDiscord;
        }

        [HarmonyPatch(typeof(Track), nameof(Track.Update))]
        [HarmonyPostfix]
        private static void TrackUpdatePostfix()
        {
            if (Input.GetKeyDown(KeyCode.F4))
            {
                SecretMode = !SecretMode;
                NotificationSystemGUI.AddMessage("Secret Mode " + (SecretMode ? "Enabled" : "Disabled"));
            }
            if (Input.GetKeyDown(KeyCode.F3))
            {
                UpdateDiscord = !UpdateDiscord;
                NotificationSystemGUI.AddMessage("Discord Rich Presence Toggled " + (UpdateDiscord ? "On" : "Off"));
            }
        }

        [HarmonyPatch(typeof(XDCustomLevelSelectMenu), "Awake")]
        [HarmonyPostfix]
        private static void XDCustomLevelSelectMenuAwakePostfix()
        {
            NotificationSystemGUI.AddMessage("Reminder: Secret Mode is currently " + (SecretMode ? "Enabled" : "Disabled"));
        }
    }
}
