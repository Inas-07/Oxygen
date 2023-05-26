using HarmonyLib;
using Oxygen.Components;
using UnityEngine;

namespace Oxygen.Patches
{
    [HarmonyPatch(typeof(FogRepeller_Sphere), nameof(FogRepeller_Sphere.StartRepelling))]
    class FogRepeller_Sphere_StartRepelling
    {
        public static void Postfix(ref FogRepeller_Sphere __instance)
        {
            if (__instance.m_infectionShield != null)
            {
                EffectVolumeManager.UnregisterVolume((EffectVolume) __instance.m_infectionShield);
                __instance.m_infectionShield.contents = eEffectVolumeContents.All;
                EffectVolumeManager.RegisterVolume((EffectVolume) __instance.m_infectionShield);
            }
        }
    }
}