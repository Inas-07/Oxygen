using UnityEngine;
using System;
using Oxygen.Config;
using GameData;
using System.Numerics;
using GTFO.API;
using Il2CppInterop.Runtime.Injection;

namespace Oxygen.Components
{
    // Align to vanilla in-game Fog Plane
    // Managed by Patches.LocalPlayerAgentSettings
    // Has nothing to do with AirManager
    // Feel free to screw up
    public class AirPlane : MonoBehaviour
    {
        public EV_Plane airPlane = null;
        private bool isAirPlaneRegistered = false;

        public AirPlane(IntPtr value) : base(value) { }

        public static void Setup()
        {
            var plane = Current;
            if (plane != null) return;

            plane = LocalPlayerAgentSettings.Current?.gameObject.AddComponent<AirPlane>();

            if (plane == null) return;

            LevelAPI.OnEnterLevel += plane.SetupAirPlane;
            LevelAPI.OnLevelCleanup += plane.OnLevelCleanup;
            LevelAPI.OnBuildStart += plane.OnLevelCleanup;

            if(GameStateManager.CurrentStateName == eGameStateName.ExpeditionFail) // checkpoint restore
            {
                plane.SetupAirPlane();
            }
        }

        internal void SetupAirPlane()
        {
            airPlane = new EV_Plane();
            uint fogsetting = RundownManager.ActiveExpedition.Expedition.FogSettings;
            if (fogsetting == 0u) fogsetting = 21u;

            OxygenBlock config;

            if (Plugin.lookup.ContainsKey(fogsetting))
            {
                config = Plugin.lookup[fogsetting];
            }
            else if (Plugin.lookup.ContainsKey(0U))
            {
                config = Plugin.lookup[0U];
            }
            else
            {
                config = null;
            }

            FogSettingsDataBlock fogSettings = GameDataBlockBase<FogSettingsDataBlock>.GetBlock(fogsetting);

            airPlane.invert = fogSettings.DensityHeightMaxBoost > fogSettings.FogDensity;
            airPlane.contents = eEffectVolumeContents.Health;
            airPlane.modification = eEffectVolumeModification.Inflict;
            airPlane.lowestAltitude = fogSettings.DensityHeightAltitude;
            airPlane.highestAltitude = fogSettings.DensityHeightAltitude + fogSettings.DensityHeightRange;

            if (config != null)
            {
                airPlane.modificationScale = config.AirLoss;
                Register();
            }
        }


        private void OnDestroy()
        {
            OnLevelCleanup();

            LevelAPI.OnEnterLevel -= SetupAirPlane;
            LevelAPI.OnLevelCleanup -= OnLevelCleanup;
            LevelAPI.OnBuildStart -= OnLevelCleanup;
        }


        private void OnLevelCleanup()
        {
            Unregister();
            isAirPlaneRegistered = false;
        }


        public void Register()
        {
            if (airPlane == null || isAirPlaneRegistered) return;

            EffectVolumeManager.RegisterVolume(airPlane);
            isAirPlaneRegistered = true;
        }


        public void Unregister()
        {
            if (airPlane == null || !isAirPlaneRegistered) return;

            EffectVolumeManager.UnregisterVolume(airPlane);
            isAirPlaneRegistered = false;
        }


        public static AirPlane? Current => LocalPlayerAgentSettings.Current?.gameObject.GetComponent<AirPlane>() ?? null;
    
        static AirPlane()
        {
            ClassInjector.RegisterTypeInIl2Cpp<AirPlane>();
        }
    }
}