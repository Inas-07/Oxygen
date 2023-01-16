using HarmonyLib;
using Oxygen.Components;
using SNetwork;
using Player;
using LevelGeneration;

namespace Oxygen.Patches
{
    [HarmonyPatch(typeof(LocalPlayerAgentSettings), nameof(LocalPlayerAgentSettings.UpdateBlendTowardsTargetFogSetting))]
    class LocalPlayerAgentSettings_UpdateBlendTowardsTargetFogSetting
    {
        public static void Postfix(LocalPlayerAgentSettings __instance, float amount)
        {
            if (__instance.m_targetFogSettings == null || !SNet.LocalPlayer.HasPlayerAgent) // consider use PlayerManager? Or adheres to mono?
                return;

            PlayerAgent playerAgent = PlayerManager.GetLocalPlayerAgent();
            if (playerAgent.FPSCamera == null || playerAgent.FPSCamera.PrelitVolume == null)
                return;

            AirPlane airPlaneCurrent = AirPlane.Current;
            if (airPlaneCurrent == null || !RundownManager.ExpeditionIsStarted)
                return;

            float num = 0.0f;
            Dimension dimension;
            if (Dimension.GetDimension(playerAgent.DimensionIndex, out dimension))
                num = dimension.GroundY;

            PreLitVolume prelitVolume = playerAgent.FPSCamera.PrelitVolume;
            airPlaneCurrent.airPlane.invert = (double)prelitVolume.m_densityHeightMaxBoost > (double)prelitVolume.m_fogDensity;
            airPlaneCurrent.airPlane.contents = eEffectVolumeContents.Health;
            airPlaneCurrent.airPlane.modification = eEffectVolumeModification.Inflict;
            airPlaneCurrent.airPlane.modificationScale = AirManager.Current.AirLoss();
            airPlaneCurrent.airPlane.lowestAltitude = prelitVolume.m_densityHeightAltitude + num;
            airPlaneCurrent.airPlane.highestAltitude = prelitVolume.m_densityHeightAltitude + prelitVolume.m_densityHeightRange + num;
        }
    }
}
