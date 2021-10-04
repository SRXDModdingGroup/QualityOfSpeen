using HarmonyLib;

namespace QualityOfSpeen.Features
{
    public class DiscordRPCFix
    {
        public static bool UpdateDiscord = true;

        [HarmonyPatch(typeof(SpinDiscord), nameof(SpinDiscord.UpdateActivityPresence))]
        [HarmonyPrefix]
        private static bool UpdateActivityPresencePrefix(ref string state, ref string details, ref string coverArt, ref string trackArtist, ref string trackTitle, ref string trackLabel, ref long endTime)
        {
            if (!UpdateDiscord) return false;

            if (Main.InGameState == InGameState.Editor)
            {
                details = "Editing " + trackTitle;
                endTime = 0;
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
}
