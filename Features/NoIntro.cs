using HarmonyLib;
using UnityEngine;

namespace QualityOfSpeen.Features
{
    public class NoIntro
    {
        private static bool SkipIntro;

        private static void Awake()
        {
            SkipIntro = Main.ModConfig.GetValueOrDefaultTo("NoIntro", "AutoSkip", false);
        }

        [HarmonyPatch(typeof(StartupScene), nameof(StartupScene.Update))]
        [HarmonyPostfix]
        private static void StartupSceneAwakePostfix(StartupScene __instance)
        {
            if (Input.anyKeyDown) SkipIntro = true;
            if (SkipIntro)
            {
                __instance._animationTimer = 8f;
            }
        }
    }
}
