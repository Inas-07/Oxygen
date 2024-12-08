using HarmonyLib;
using Oxygen.Components;
using SNetwork;
using Player;
using LevelGeneration;
using UnityEngine;

namespace Oxygen.Patches
{
    // handle on-going fog transition
    // fully rewrite LocalPlayerAgentSettings.UpdateBlendTowardsTargetFogSetting, 
    // and add codes for handling oxygen
    [HarmonyPatch]
    class LocalPlayerAgentSettings_UpdateBlendTowardsTargetFogSetting
    {
        private static void VanillaCode(LocalPlayerAgentSettings __instance, float amount)
        {

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LocalPlayerAgentSettings), nameof(LocalPlayerAgentSettings.UpdateBlendTowardsTargetFogSetting))]
        public static bool Pre_UpdateBlendTowardsTargetFogSetting(LocalPlayerAgentSettings __instance, float amount)
        {
            // ========================================================================================
            //                                      R6 mono code 
            // ========================================================================================
            if (!RundownManager.ExpeditionIsStarted) return true;
            if (__instance.m_targetFogSettings == null || !SNet.LocalPlayer.HasPlayerAgent) return true;

            PlayerAgent playerAgent = PlayerManager.GetLocalPlayerAgent();
            if (playerAgent.FPSCamera == null || playerAgent.FPSCamera.PrelitVolume == null) return true;

            float dimHeight = 0f;
            if (Dimension.GetDimension(playerAgent.DimensionIndex, out var dimension))
            {
                dimHeight = dimension.GroundY; 
            }

            PreLitVolume prelitVolume = playerAgent.FPSCamera.PrelitVolume;
            prelitVolume.m_fogColor = Color.Lerp(__instance.m_fogSettings.FogColor, __instance.m_targetFogSettings.FogColor, amount);
            prelitVolume.m_fogDensity = Mathf.Lerp(__instance.m_fogSettings.FogDensity, __instance.m_targetFogSettings.FogDensity, amount);
            prelitVolume.m_densityNoiseDirection = Vector3.Lerp(__instance.m_fogSettings.DensityNoiseDirection, __instance.m_targetFogSettings.DensityNoiseDirection, amount);
            prelitVolume.m_densityNoiseSpeed = Mathf.Lerp(__instance.m_fogSettings.DensityNoiseSpeed, __instance.m_targetFogSettings.DensityNoiseSpeed, amount);
            prelitVolume.m_densityNoiseScale = Mathf.Lerp(__instance.m_fogSettings.DensityNoiseScale, __instance.m_targetFogSettings.DensityNoiseScale, amount);

            // the following 1 line has been modified 
            prelitVolume.m_densityHeightAltitude = Mathf.Lerp(__instance.m_fogSettings.DensityHeightAltitude, __instance.m_targetFogSettings.DensityHeightAltitude + dimHeight, amount);
            prelitVolume.m_densityHeightRange = Mathf.Lerp(__instance.m_fogSettings.DensityHeightRange, __instance.m_targetFogSettings.DensityHeightRange, amount);
            prelitVolume.m_densityHeightMaxBoost = Mathf.Lerp(__instance.m_fogSettings.DensityHeightMaxBoost, __instance.m_targetFogSettings.DensityHeightMaxBoost, amount);

            __instance.currentInfection = Mathf.Lerp(__instance.m_fogSettings.Infection, __instance.m_targetFogSettings.Infection, amount);
            __instance.infectionPlane.invert = (prelitVolume.m_densityHeightMaxBoost > prelitVolume.m_fogDensity);
            __instance.infectionPlane.modificationScale = __instance.currentInfection;

            // the following 2 lines has been modified 
            __instance.infectionPlane.lowestAltitude = prelitVolume.m_densityHeightAltitude;
            __instance.infectionPlane.highestAltitude = prelitVolume.m_densityHeightAltitude + prelitVolume.m_densityHeightRange;

            if (!__instance.isInfectionPlaneRegistered)
            {
                if (__instance.currentInfection > 0f)
                {
                    EffectVolumeManager.RegisterVolume(__instance.infectionPlane);
                    __instance.isInfectionPlaneRegistered = true;
                }
            }
            else if (__instance.currentInfection <= 0f)
            {
                EffectVolumeManager.UnregisterVolume(__instance.infectionPlane);
                __instance.isInfectionPlaneRegistered = false;
            }

            // ========================================================================================
            //                                      oxygen
            // ========================================================================================


            if (!AirManager.Current.HasConfig)
            {
                AirPlane.Current.Unregister();
                return false;
            }

            AirPlane airPlaneCurrent = AirPlane.Current;
            if (airPlaneCurrent == null) return false; 
            
            airPlaneCurrent.airPlane.invert = (double)prelitVolume.m_densityHeightMaxBoost > (double)prelitVolume.m_fogDensity;
            airPlaneCurrent.airPlane.contents = eEffectVolumeContents.Health;
            airPlaneCurrent.airPlane.modification = eEffectVolumeModification.Inflict;
            airPlaneCurrent.airPlane.modificationScale = AirManager.Current.AirLoss();
            airPlaneCurrent.airPlane.lowestAltitude = prelitVolume.m_densityHeightAltitude + dimHeight;
            airPlaneCurrent.airPlane.highestAltitude = prelitVolume.m_densityHeightAltitude + prelitVolume.m_densityHeightRange;

            AirPlane.Current.Register();
            return false;
        }
    }
}
