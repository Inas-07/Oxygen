using HarmonyLib;
using Oxygen.Components;
using Player;

namespace Oxygen.Patches
{
    [HarmonyPatch]
    class Patch_PlayerAgent
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerAgent), nameof(PlayerAgent.ReceiveModification))]
        public static void ReceiveModification(PlayerAgent __instance, ref EV_ModificationData data)
        {
            if (!AirManager.Current.HasAirConfig()) return;

            if (data.health != 0.0)
            {
                AirManager.Current.RemoveAir(data.health);
            }
            else
            {
                AirManager.Current.AddAir();
            }
            
            // Prevent not implemented error
            data.health = 0.0f;
        }

        [HarmonyPostfix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(PlayerAgent), nameof(PlayerAgent.Setup))]
        internal static void Post_Setup(PlayerAgent __instance)
        {
            if (!__instance.IsLocallyOwned)
                return;
            AirManager.Setup(__instance);
        }
    }
}