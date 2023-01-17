using UnityEngine;
using System;
using GameData;
using Player;
using Oxygen.Utils;


namespace Oxygen.Components
{
    // Align to vanilla in-game Fog Plane
    // Managed by Patches.LocalPlayerAgentSettings
    public class AirPlane : MonoBehaviour
    {
        public static AirPlane Current = null;
        public EV_Plane airPlane = null;
        public bool isAirPlaneRegistered = false;

        public AirPlane(IntPtr value) : base(value)
        {
        }
        
        public static void Setup()
        {
            if(Current == null)
            {
                Current = LocalPlayerAgentSettings.Current.gameObject.AddComponent<AirPlane>();
                Current.airPlane = new EV_Plane();
            }
        }

        //public void OnExpeditionStarted()
        //{
        //    ExpeditionInTierData activeExpedition = RundownManager.ActiveExpedition;
            
        //    if (activeExpedition != null && activeExpedition.Expedition.FogSettings > 0U)
        //    {
        //        SetAirPlane(GameDataBlockBase<FogSettingsDataBlock>.GetBlock(activeExpedition.Expedition.FogSettings));
        //    }
        //    else
        //    {
        //        Log.Error("FogSetting unspecified, will not apply Oxygen.");
        //    }
        //}
        
        //public void SetAirPlane(FogSettingsDataBlock fogSettings)
        //{
        //    if (AirManager.Current.AirLoss() > 0.0f)
        //    {
        //        airPlane.invert = (double) fogSettings.DensityHeightMaxBoost > (double) fogSettings.FogDensity;
        //        airPlane.contents = eEffectVolumeContents.Health;
        //        airPlane.modification = eEffectVolumeModification.Inflict;
        //        airPlane.modificationScale = AirManager.Current.AirLoss();
        //        airPlane.lowestAltitude = fogSettings.DensityHeightAltitude;
        //        airPlane.highestAltitude = fogSettings.DensityHeightAltitude + fogSettings.DensityHeightRange;
        //        EffectVolumeManager.RegisterVolume((EffectVolume) this.airPlane);
        //    }
        //    else
        //    {
        //        EffectVolumeManager.UnregisterVolume((EffectVolume) this.airPlane);
        //    }
        //}
    }
}