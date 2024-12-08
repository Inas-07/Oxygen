using HarmonyLib;
using Oxygen.Components;
using Player;


namespace Oxygen.Patches
{
    [HarmonyPatch(typeof(EnvironmentStateManager), nameof(EnvironmentStateManager.UpdateFog))]
    class EnvironmentStateManager_UpdateFog
    {
        public static void Prefix(EnvironmentStateManager __instance)
        {
            if (AirManager.Current == null) return;

            FogState fogState = __instance.m_stateReplicator.State.FogStates[__instance.m_latestKnownLocalDimensionCreationIndex];

            if (fogState.FogDataID <= 0u) return;

            AirManager.Current.UpdateAirConfig(fogState.FogDataID);

            if(!AirManager.Current.HasConfig) 
            {
                AirManager.Current.StopInfectionLoop();
            }
        }
    }
}
