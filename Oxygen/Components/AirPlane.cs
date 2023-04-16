using UnityEngine;
using System;
using Oxygen.Config;
using GameData;

namespace Oxygen.Components
{
    // Align to vanilla in-game Fog Plane
    // Managed by Patches.LocalPlayerAgentSettings
    // Has nothing to do with AirManager
    // Feel free to screw up
    public class AirPlane : MonoBehaviour
    {
        public static AirPlane Current = null;
        public EV_Plane airPlane = null;
        private bool isAirPlaneRegistered = false;

        public AirPlane(IntPtr value) : base(value)
        {
        }
        
        public static void OnBuildStart() // init air plane
        {
            if(Current == null)
            {
                Current = LocalPlayerAgentSettings.Current.gameObject.AddComponent<AirPlane>();
            }

            Current.airPlane = new EV_Plane();
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

            Current.airPlane.invert = fogSettings.DensityHeightMaxBoost > fogSettings.FogDensity;
            Current.airPlane.contents = eEffectVolumeContents.Health;
            Current.airPlane.modification = eEffectVolumeModification.Inflict;
            Current.airPlane.lowestAltitude = fogSettings.DensityHeightAltitude;
            Current.airPlane.highestAltitude = fogSettings.DensityHeightAltitude + fogSettings.DensityHeightRange;

            if (config != null)
            {
                Current.airPlane.modificationScale = config.AirLoss;
                Current.Register();
            }
        }

        public static void OnLevelCleanup()
        {
            if (Current == null) return;

            Current.Unregister();
            Current.isAirPlaneRegistered = false;
            Current.airPlane = null;
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
    }
}