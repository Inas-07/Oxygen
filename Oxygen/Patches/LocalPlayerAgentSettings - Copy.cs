//using HarmonyLib;
//using Oxygen.Components;
//using SNetwork;
//using Player;
//using LevelGeneration;
//using Oxygen.Utils;

//namespace Oxygen.Patches
//{
//    // handle on-going fog transition
//    // 
//    [HarmonyPatch(typeof(LocalPlayerAgentSettings), nameof(LocalPlayerAgentSettings.UpdateBlendTowardsTargetFogSetting))]
//    class LocalPlayerAgentSettings_UpdateBlendTowardsTargetFogSetting
//    {
//        public static void Postfix(LocalPlayerAgentSettings __instance, float amount)
//        {
//            if (!RundownManager.ExpeditionIsStarted) return;
//            if (__instance.m_targetFogSettings == null || !SNet.LocalPlayer.HasPlayerAgent) return;

//            PlayerAgent playerAgent = PlayerManager.GetLocalPlayerAgent();
//            if (playerAgent.FPSCamera == null || playerAgent.FPSCamera.PrelitVolume == null) return;

//            //OxygenLogger.Warning("fixing....");

//            PreLitVolume prelitVolume = playerAgent.FPSCamera.PrelitVolume;
//            float dim_height = 0.0f;
//            Dimension dimension;
//            if (Dimension.GetDimension(playerAgent.DimensionIndex, out dimension))
//                dim_height = dimension.GroundY;

//            //var infectionPlane = LocalPlayerAgentSettings.Current.infectionPlane;
//            //infectionPlane.lowestAltitude = prelitVolume.m_densityHeightAltitude + dim_height;
//            //infectionPlane.highestAltitude = prelitVolume.m_densityHeightAltitude + dim_height + prelitVolume.m_densityHeightRange;

//            if (!AirManager.Current.HasAirConfig())
//            {
//                AirPlane.Current.Unregister();
//                return;
//            }

//            AirPlane airPlaneCurrent = AirPlane.Current;
//            if (airPlaneCurrent == null) return;
            
//            airPlaneCurrent.airPlane.invert = (double)prelitVolume.m_densityHeightMaxBoost > (double)prelitVolume.m_fogDensity;
//            airPlaneCurrent.airPlane.contents = eEffectVolumeContents.Health;
//            airPlaneCurrent.airPlane.modification = eEffectVolumeModification.Inflict;
//            airPlaneCurrent.airPlane.modificationScale = AirManager.Current.AirLoss();
//            airPlaneCurrent.airPlane.lowestAltitude = prelitVolume.m_densityHeightAltitude + dim_height;
//            airPlaneCurrent.airPlane.highestAltitude = prelitVolume.m_densityHeightAltitude + prelitVolume.m_densityHeightRange + dim_height;

//            AirPlane.Current.Register();
//        }
//    }
//}
