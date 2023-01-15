using UnityEngine;
using System;
using GameData;
using Player;
using Oxygen.Utils;


namespace Oxygen.Components
{
    // Align to vanilla in-game Fog Plane
    public class AirPlane : MonoBehaviour
    {
        public static AirPlane Current = null;
        public EV_Plane airPlane = null;

        public AirPlane(IntPtr value) : base(value)
        {
        }
        
        public static void Setup()
        {
            if(Current == null)
            {
                Current = LocalPlayerAgentSettings.Current.gameObject.AddComponent<AirPlane>();
            }
            Current.OnExpeditionStarted();
        }

        public void OnExpeditionStarted()
        {
            airPlane = new EV_Plane();
            ExpeditionInTierData activeExpedition = RundownManager.ActiveExpedition;
            
            if (activeExpedition != null && activeExpedition.Expedition.FogSettings > 0U)
            {
                SetAirPlane(GameDataBlockBase<FogSettingsDataBlock>.GetBlock(activeExpedition.Expedition.FogSettings));
            }
            else
            {
                Log.Error("FogSetting unspecified, will not apply Oxygen.");
            }
        }
        
        public void SetAirPlane(FogSettingsDataBlock fogSettings)
        {
            if (AirManager.Current.config.AirLoss > 0.0f)
            {
                this.airPlane.invert = (double) fogSettings.DensityHeightMaxBoost > (double) fogSettings.FogDensity;
                this.airPlane.contents = eEffectVolumeContents.Health;
                this.airPlane.modification = eEffectVolumeModification.Inflict;
                this.airPlane.modificationScale = AirManager.Current.config.AirLoss;
                this.airPlane.lowestAltitude = fogSettings.DensityHeightAltitude;
                this.airPlane.highestAltitude = fogSettings.DensityHeightAltitude + fogSettings.DensityHeightRange;
                EffectVolumeManager.RegisterVolume((EffectVolume) this.airPlane);
            }
            else
            {
                EffectVolumeManager.UnregisterVolume((EffectVolume) this.airPlane);
            }
        }
    }
}