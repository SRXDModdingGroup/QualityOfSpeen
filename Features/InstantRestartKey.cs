using HarmonyLib;

namespace QualityOfSpeen.Features
{
    public class InstantRestartKey
    {
        public static bool currentKeyState = false, previousKeyState = false;

        [HarmonyPatch(typeof(Track), nameof(Track.Update))]
        [HarmonyPostfix]
        private static void RestartKey()
        {
            previousKeyState = currentKeyState;
            currentKeyState = XDInputModule.Instance.xdInput.GetButtonDown(InputMapping.SpinCommands.RestartSong);
            if (currentKeyState && !previousKeyState && Main.InGameState == InGameState.Playing && !Main.IsDead && !Main.HasWon && !Main.IsRestarting)
            {
                Track.Instance.RestartTrack();
            }
        }
    }
}
