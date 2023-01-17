using UnityEngine;
using System;
using Player;
using AK;
using GameData;
using Oxygen.Config;
using GTFO.API;
namespace Oxygen.Components
{
    public class AirManager : MonoBehaviour
    {
        public static AirManager Current = null;
        private PlayerAgent m_playerAgent;
        private HUDGlassShatter m_hudGlass;
        private Dam_PlayerDamageBase Damage;
        public OxygenBlock config = null;
        private uint fogSetting = 0u;
        private FogSettingsDataBlock fogSettingDB = null;

        private float airAmount = 1f;
        private float damageTick = 0f;
        private float glassShatterAmount = 0f;
        private bool m_isInInfectionLoop = false;

        public AirManager(IntPtr value) : base(value) { }

        public static void Setup()
        {
            if (Current == null)
            {
                Current = PlayerManager.Current.m_localPlayerAgentInLevel.gameObject.AddComponent<AirManager>();
                //AirBar.Setup();
                //AirPlane.Setup();
                LevelAPI.OnBuildDone += () => AirBar.Current.UpdateAirText(Current.config);
            }

            Current.UpdateAirConfig(RundownManager.ActiveExpedition.Expedition.FogSettings);
        }

        void Awake()
        {
            m_playerAgent = PlayerManager.GetLocalPlayerAgent();
            m_hudGlass = m_playerAgent.FPSCamera.GetComponent<HUDGlassShatter>();
            Damage = m_playerAgent.gameObject.GetComponent<Dam_PlayerDamageBase>();
        }
        
        void Update()
        {
            if (!RundownManager.ExpeditionIsStarted) return;
            if (!HasAirConfig())
            {
                AirBar.Current.SetVisible(false);
                return;
            }

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
            if (!HasAirConfig()) return;

            float amount = this.config.AirGain;
            airAmount = Mathf.Clamp01(airAmount + amount);
            AirBar.Current.UpdateAirBar(airAmount);

            // If fogSettingDB.Infection > 0, infection effect sound plays via vanilla code.
            if (this.fogSettingDB.Infection <= 0.0f && this.m_isInInfectionLoop)
            {
                StopInfectionLoop();
            }
        }

        public void RemoveAir(float amount) 
        {
            if (!HasAirConfig()) return;

            //airAmount = Mathf.Clamp01(airAmount - amount);
            // `amount` doesn't update when using LiveEdit 
            // so I changed to config.amount
            amount = this.config.AirLoss;

            airAmount = Mathf.Clamp01(airAmount - amount);
            AirBar.Current.UpdateAirBar(airAmount);

            // If fogSettingDB.Infection > 0, infection effect sound plays via vanilla code.
            if (this.fogSettingDB.Infection <= 0.0f && amount > 0.0f)
            {
                StartInfectionLoop();
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
            if (fogsetting == 0u)
                fogsetting = this.fogSetting;

            if (Plugin.lookup.ContainsKey(fogsetting))
            {
                this.config = Plugin.lookup[fogsetting];
            }
            else if (Plugin.lookup.ContainsKey(0U))
            {
                this.config = Plugin.lookup[0U];
            }
            else
            {
                this.config = null;
                airAmount = 1.0f; // no air config. reset air amount
            }

            this.fogSetting = fogsetting;
            this.fogSettingDB = FogSettingsDataBlock.GetBlock(fogsetting);
        }

        public float AirLoss() => config == null ? 0f : config.AirLoss;

        public bool AlwaysDisplayAirBar() => config == null ? false : config.AlwaysDisplayAirBar;

        public uint FogSetting() => fogSetting;

        public string AirText() => config == null ? null : config.AirText.Text;

        public float AirTextX() => config == null ? 0.0f : config.AirText.x;
        public float AirTextY() => config == null ? 0.0f : config.AirText.y;

        public bool HasAirConfig() => config != null;

        public void StartInfectionLoop()
        {
            if (m_isInInfectionLoop) return;

            m_playerAgent.Sound.Post(EVENTS.INFECTION_EFFECT_LOOP_START);
            m_isInInfectionLoop = true;
        }

        public void StopInfectionLoop()
        {
            if (!m_isInInfectionLoop) return;

            m_playerAgent.Sound.Post(EVENTS.INFECTION_EFFECT_LOOP_STOP);
            m_isInInfectionLoop = false;
        }

    }
}