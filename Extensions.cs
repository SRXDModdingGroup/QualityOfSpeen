using HarmonyLib;
using System.Reflection;

namespace QualityOfSpeen
{
    public static class Extensions
    {
        public static void PatchAll<T>(this Harmony harmony) => harmony.PatchAll(typeof(T));

        // Source: https://stackoverflow.com/a/46488844
        public static T GetFieldValue<T>(this object obj, string name)
        {
            // Set the flags so that private and public fields from instances will be found
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var field = obj.GetType().GetField(name, bindingFlags);
            return (T)field?.GetValue(obj);
        }
    }
}
