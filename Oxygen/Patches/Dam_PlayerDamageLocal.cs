using HarmonyLib;

namespace Oxygen.Patches
{
    [HarmonyPatch(typeof(Dam_PlayerDamageLocal), nameof(Dam_PlayerDamageLocal.ReceiveNoAirDamage))]
    class Dam_PlayerDamageLocal_ReceiveNoAirDamage
    {
        public static bool Prefix(Dam_PlayerDamageLocal __instance, pMiniDamageData data)
        {
            __instance.OnIncomingDamage(data.damage.Get(__instance.HealthMax), data.damage.Get(__instance.HealthMax), false);
            __instance.Hitreact(data.damage.Get(__instance.HealthMax), UnityEngine.Vector3.zero, triggerCameraShake: true, triggerGenericDialog: false);
            return false;
        }
    }
}
