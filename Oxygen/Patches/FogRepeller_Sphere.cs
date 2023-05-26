using HarmonyLib;

namespace Oxygen.Patches
{
    [HarmonyPatch(typeof(FogRepeller_Sphere), nameof(FogRepeller_Sphere.StartRepelling))]
    class FogRepeller_Sphere_StartRepelling
    {
        public static void Postfix(ref FogRepeller_Sphere __instance)
        {
            if (__instance.m_infectionShield != null)
            {
                EffectVolumeManager.UnregisterVolume(__instance.m_infectionShield);
                __instance.m_infectionShield.contents = eEffectVolumeContents.All;
                EffectVolumeManager.RegisterVolume(__instance.m_infectionShield);
            }
        }
    }
}