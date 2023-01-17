using UnityEngine;
using System;
using Player;
using AK;
using GameData;
using Oxygen.Config;

namespace Oxygen.Components
{
    public class AirManager : MonoBehaviour
    {
        public static AirManager Current = null;
        private PlayerAgent m_playerAgent;
        private HUDGlassShatter m_hudGlass;
        private Dam_PlayerDamageBase Damage;
        private OxygenBlock config = new();
        private uint fogSetting = 0u;
        private FogSettingsDataBlock fogSettingDB = null;

        private float airAmount = 1f;
        private float damageTick = 0f;
        private float glassShatterAmount = 0f;
        private bool m_isInInfectionLoop = false;

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

            uint fogsetting = RundownManager.ActiveExpedition.Expedition.FogSettings;
            UpdateAirConfig(fogsetting);
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
                m_playerAgent.Breathing.m_currentBreathingIntensity = 0;
            }
            else if (airAmount > 0.6f)
            {
                m_playerAgent.Breathing.m_currentBreathingIntensity = 1;
                PlayerDialogManager.WantToStartDialog(173U, m_playerAgent);
            }
            else if (airAmount > 0.4f)
            {
                m_playerAgent.Breathing.m_currentBreathingIntensity = 2;
                PlayerDialogManager.WantToStartDialog(173U, m_playerAgent);
            }
            else 
            {
                m_playerAgent.Breathing.m_currentBreathingIntensity = 3;
                PlayerDialogManager.WantToStartDialog(173U, m_playerAgent);
            }

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

            // If fogSettingDB.Infection > 0, infection effect sound plays via vanilla code.
            if (this.fogSettingDB.Infection <= 0.0f && this.m_isInInfectionLoop)
            {
                m_playerAgent.Sound.Post(EVENTS.INFECTION_EFFECT_LOOP_STOP);
                this.m_isInInfectionLoop = false;
            }
        }

        public void RemoveAir(float amount) 
        {
            // `amount` doesn't update when using LiveEdit 
            // so I changed to config.amount

            //airAmount = Mathf.Clamp01(airAmount - amount);
            if (this.config != null) amount = this.config.AirLoss;


            airAmount = Mathf.Clamp01(airAmount - amount);
            AirBar.Current.UpdateAirBar(airAmount);

            // If fogSettingDB.Infection > 0, infection effect sound plays via vanilla code.
            if (this.fogSettingDB.Infection <= 0.0f && amount > 0.0f)
            {
                if(!this.m_isInInfectionLoop)
                {
                    m_playerAgent.Sound.Post(EVENTS.INFECTION_EFFECT_LOOP_START);
                    this.m_isInInfectionLoop = true;
                }
            }
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

        public void UpdateAirConfig(uint fogsetting)
        {
            //Log.Warning($"Updating to fog {fogsetting}");

            if (fogsetting == 0u)
                fogsetting = this.fogSetting;

            if (Plugin.lookup.ContainsKey(fogsetting))
            {
                this.config = Plugin.lookup[fogsetting];
                //Log.Warning("Find config for this fogsetting!");
            }
            else if (Plugin.lookup.ContainsKey(0U))
            {
                this.config = Plugin.lookup[0U];
            }
            else
            {
                this.config = new();
            }

            this.fogSetting = fogsetting;
            this.fogSettingDB = FogSettingsDataBlock.GetBlock(fogsetting);
        }

        public float AirLoss() => this.config == null ? 0f : this.config.AirLoss;

        public bool AlwaysDisplayAirBar() => this.config == null ? false : config.AlwaysDisplayAirBar;

        public uint FogSetting() => fogSetting;

        public AirText AirText() => config.AirText;
    }
}