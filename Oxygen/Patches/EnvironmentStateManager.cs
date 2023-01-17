using HarmonyLib;
using Oxygen.Components;


namespace Oxygen.Patches
{
    [HarmonyPatch(typeof(EnvironmentStateManager), nameof(EnvironmentStateManager.AttemptStartFogTransition))]
    class EnvironmentStateManager_AttemptStartFogTransition
    {
        public static void Postfix(uint fogDataId)
        {
            if (AirManager.Current == null) return;

            AirManager.Current.UpdateAirConfig(fogDataId);
            AirBar.Current.UpdateAirText(AirManager.Current.config);

            if(AirManager.Current != null && !AirManager.Current.HasAirConfig()) 
            {
                AirManager.Current.StopInfectionLoop();
            }
        }
    }
}
