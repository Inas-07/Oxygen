using GameEvent;
using HarmonyLib;
using Oxygen.Components;
using Oxygen.Utils;
using SNetwork;

namespace Oxygen.Patches
{
    [HarmonyPatch]
    class Patches_Dam_PlayerDamageLocal
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Dam_PlayerDamageLocal), nameof(Dam_PlayerDamageLocal.ReceiveNoAirDamage))]
        public static bool Pre_ReceiveNoAirDamage(Dam_PlayerDamageLocal __instance, pMiniDamageData data)
        {
            // this method sucks
            //__instance.OnIncomingDamage(data.damage.Get(__instance.HealthMax), data.damage.Get(__instance.HealthMax));

            float damage = data.damage.Get(__instance.HealthMax);
            __instance.m_nextRegen = Clock.Time + __instance.Owner.PlayerData.healthRegenStartDelayAfterDamage;
            if (__instance.Owner.IsLocallyOwned)
            {
                DramaManager.CurrentState.OnLocalDamage(damage);
                GameEventManager.PostEvent(eGameEvent.player_take_damage, __instance.Owner, damage);
            }
            else
                DramaManager.CurrentState.OnTeammatesDamage(damage);
            if (__instance.IgnoreAllDamage)
                return false;
            if (SNet.IsMaster)
            {
                bool flag = __instance.RegisterDamage(damage);
                if (flag)
                    __instance.SendSetDead();
                else
                    __instance.SendSetHealth(__instance.Health);
            }

            __instance.Hitreact(data.damage.Get(__instance.HealthMax), UnityEngine.Vector3.zero, triggerCameraShake: true, triggerGenericDialog: AirManager.Current.HealthToRegen() > 0.0f);
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Dam_PlayerDamageLocal), nameof(Dam_PlayerDamageLocal.ReceiveBulletDamage))]
        public static void Post_ReceiveBulletDamage()
        {
            AirManager.Current.ResetHealthToRegen();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Dam_PlayerDamageLocal), nameof(Dam_PlayerDamageLocal.ReceiveMeleeDamage))]
        public static void Post_ReceiveMeleeDamage()
        {
            AirManager.Current.ResetHealthToRegen();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Dam_PlayerDamageLocal), nameof(Dam_PlayerDamageLocal.ReceiveFireDamage))]
        public static void Post_ReceiveFireDamage()
        {
            AirManager.Current.ResetHealthToRegen();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Dam_PlayerDamageLocal), nameof(Dam_PlayerDamageLocal.ReceiveShooterProjectileDamage))]
        public static void Post_ReceiveShooterProjectileDamage()
        {
            AirManager.Current.ResetHealthToRegen();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Dam_PlayerDamageLocal), nameof(Dam_PlayerDamageLocal.ReceiveTentacleAttackDamage))]
        public static void Post_ReceiveTentacleAttackDamage()
        {
            AirManager.Current.ResetHealthToRegen();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Dam_PlayerDamageLocal), nameof(Dam_PlayerDamageLocal.ReceivePushDamage))]
        public static void Post_ReceivePushDamage()
        {
            AirManager.Current.ResetHealthToRegen();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Dam_PlayerDamageLocal), nameof(Dam_PlayerDamageLocal.ReceiveSetDead))]
        public static void Post_ReceiveSetDead()
        {
            AirManager.Current.ResetHealthToRegen();
        }
    }
}
