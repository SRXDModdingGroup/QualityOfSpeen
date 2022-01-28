using HarmonyLib;

namespace QualityOfSpeen.Features
{
    public class InstantRestartKey
    {
        public static bool currentKeyState = false, previousKeyState = false;

        private static XDInput inputRef;

        [HarmonyPatch(typeof(Track), nameof(Track.Update))]
        [HarmonyPostfix]
        private static void RestartKey()
        {
            if (inputRef == null)
                inputRef = XDInputModule.Instance.GetFieldValue<XDInput>("xdInput");
            previousKeyState = currentKeyState;
            currentKeyState = inputRef.GetButtonDown(InputMapping.SpinCommands.RestartSong);
            if (currentKeyState && !previousKeyState && Main.InGameState == InGameState.Playing && !Main.IsDead && !Main.HasWon && !Main.IsRestarting)
            {
                Track.Instance.RestartTrack();
            }
        }
    }
}
