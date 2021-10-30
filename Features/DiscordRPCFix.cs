using HarmonyLib;
using UnityEngine;

namespace QualityOfSpeen.Features
{
    public class DiscordRPCFix
    {
        public static bool UpdateDiscord;
        public static bool SecretMode;

        private static void Awake()
        {
            UpdateDiscord = Main.ModConfig.GetValueOrDefaultTo("Discord", "EnableRichPresence", true);
            SecretMode = Main.ModConfig.GetValueOrDefaultTo("Discord", "StartInSecretMode", false);
        }

        [HarmonyPatch(typeof(SpinDiscord), nameof(SpinDiscord.UpdateActivityPresence))]
        [HarmonyPrefix]
        private static bool UpdateActivityPresencePrefix(ref string state, ref string details, ref string coverArt, ref string trackArtist, ref string trackTitle, ref string trackLabel, ref long endTime)
        {
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

        [HarmonyPatch(typeof(Track), nameof(Track.Update))]
        [HarmonyPostfix]
        private static void TrackUpdatePostfix()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                SecretMode = !SecretMode;
                Main.Logger.LogWarning("Secret Mode " + (SecretMode ? "Enabled" : "Disabled"));
            }
        }

        [HarmonyPatch(typeof(XDCustomLevelSelectMenu), nameof(XDCustomLevelSelectMenu.Awake))]
        [HarmonyPostfix]
        private static void XDCustomLevelSelectMenuAwakePostfix()
        {
            Main.Logger.LogWarning("Reminder: Secret Mode is currently " + (SecretMode ? "Enabled" : "Disabled"));
        }
    }
}
