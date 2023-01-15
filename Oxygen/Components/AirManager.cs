using UnityEngine;
using System;
using Oxygen.Utils;
using Player;

namespace Oxygen.Components
{
    public class AirManager : MonoBehaviour
    {
        public static AirManager Current = null;
        private PlayerAgent m_playerAgent;
        private HUDGlassShatter m_hudGlass;
        private Dam_PlayerDamageBase Damage;
        public OxygenBlock config = new();
        
        private float airAmount = 1f;
        private float damageTick = 0f;
        private float glassShatterAmount = 0f;
        
        public AirManager(IntPtr value) : base(value) { }

        public static void Setup()
        {
            if(Current == null) AirManager.Current =
                    PlayerManager.Current.m_localPlayerAgentInLevel.gameObject.AddComponent<AirManager>();
        }

        void Awake()
        {
            m_playerAgent = PlayerManager.GetLocalPlayerAgent();
            m_hudGlass = m_playerAgent.FPSCamera.GetComponent<HUDGlassShatter>();
            Damage = m_playerAgent.gameObject.GetComponent<Dam_PlayerDamageBase>();

            uint pid = RundownManager.ActiveExpedition.LevelLayoutData;
            if (Plugin.lookup.ContainsKey(pid))
            {
                config = Plugin.lookup[pid];
            }
            else if (Plugin.lookup.ContainsKey(0U))
            {
                config = Plugin.lookup[0U];
            }
        }
        
        void Update()
        {
            if (!RundownManager.ExpeditionIsStarted) return;

            // Breathing intensity, Coughing, and Damage Tick
            if (airAmount == 1f)
            {
                if(config.AlwaysDisplayAirBar)
                {
                    AirBar.Current.SetVisible(true);
                }
                else
                {
                    AirBar.Current.SetVisible(false);
                }
            }
            else
            {
                AirBar.Current.SetVisible(true);
            }
            
            if (airAmount > 0.8f && airAmount <= 1.0f)
            {
                //m_playerAgent.Breathing.TryChangeBreathingIntensity(0);
                m_playerAgent.Breathing.m_currentBreathingIntensity = 0;

            }
            else if (airAmount > 0.6f)
            {
                //m_playerAgent.Breathing.TryChangeBreathingIntensity(1);
                m_playerAgent.Breathing.m_currentBreathingIntensity = 1;
                PlayerDialogManager.WantToStartDialog(174U, m_playerAgent);
            }
            else if (airAmount > 0.4f)
            {
                //m_playerAgent.Breathing.TryChangeBreathingIntensity(2);
                m_playerAgent.Breathing.m_currentBreathingIntensity = 2;
                PlayerDialogManager.WantToStartDialog(174U, m_playerAgent);
            }
            else //if (airAmount > 0.2)
            {
                //m_playerAgent.Breathing.TryChangeBreathingIntensity(3);
                m_playerAgent.Breathing.m_currentBreathingIntensity = 3;
                PlayerDialogManager.WantToStartDialog(173U, m_playerAgent);
            }
            //else if (airAmount < 0.2f)
            //{
            //    damageTick += Time.deltaTime;
            //    PlayerDialogManager.WantToStartDialog(173U, m_playerAgent);
            //}

            if (airAmount <= config.DamageThreshold)
            {
                damageTick += Time.deltaTime;
            }

            if (damageTick > config.DamageTime)
            {
                AirDamage();
            }
        }

        public void AddAir()
        {
            float amount = this.config.AirGain;
            airAmount = Mathf.Clamp01(airAmount + amount);
            AirBar.Current.UpdateAirBar(airAmount);
        }

        public void RemoveAir(float amount)
        {
            airAmount = Mathf.Clamp01(airAmount - amount);
            AirBar.Current.UpdateAirBar(airAmount);
        }

        public void AirDamage()
        {
            Damage.NoAirDamage(config.DamageAmount);

            if (config.ShatterGlass)
            {
                glassShatterAmount += config.ShatterAmount;
                m_hudGlass.SetGlassShatterProgression(glassShatterAmount); 
            }
                
            damageTick = 0f;
        }
    }
}