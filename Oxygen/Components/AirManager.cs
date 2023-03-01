using UnityEngine;
using System;
using Player;
using AK;
using GameData;
using Oxygen.Config;
using GTFO.API;
namespace Oxygen.Components
{
    // AirManager doesn't handle environment state change.
    // We need to tell the AirManager about the change in Patches
    // Controls air bar
    public class AirManager : MonoBehaviour
    {
        public static AirManager Current = null;

        // 
        public PlayerAgent m_playerAgent;

        private HUDGlassShatter m_hudGlass;
        private Dam_PlayerDamageBase Damage;

        // air config
        public OxygenBlock config = null;
        private uint fogSetting = 0u;
        private FogSettingsDataBlock fogSettingDB = null;
        private float airAmount = 1f;
        private float damageTick = 0f;
        private float glassShatterAmount = 0f;
        private bool m_isInInfectionLoop = false;
        private float healthToRegen = 0f;
        private float healthRegenTick = 0.0f;
        private float healthRegenDelay = 0.25f;
        private float tickUntilRegenHealth = 0.0f;

        public AirManager(IntPtr value) : base(value) { }

        public static void Setup()
        {
            if (Current == null)
            {
                Current = PlayerManager.Current.m_localPlayerAgentInLevel.gameObject.AddComponent<AirManager>();
            }
        }

        public static void OnBuildDone()
        {
            if (Current == null) return;

            Current.m_playerAgent = PlayerManager.GetLocalPlayerAgent();
            Current.m_hudGlass = Current.m_playerAgent.FPSCamera.GetComponent<HUDGlassShatter>();
            Current.Damage = Current.m_playerAgent.gameObject.GetComponent<Dam_PlayerDamageBase>();

            Current.UpdateAirConfig(RundownManager.ActiveExpedition.Expedition.FogSettings);
            
            AirBar.Current.UpdateAirText(Current.config);
        }

        public static void OnLevelCleanup()
        {
            if (Current == null) return;

            if (Current.m_isInInfectionLoop)
            {
                Current.StopInfectionLoop();
            }

            Current.config = null;
            Current.fogSetting = 0u;
            Current.fogSettingDB = null;
            Current.airAmount = 0f;
            Current.damageTick = 0f;
            Current.glassShatterAmount = 0f;
            Current.healthToRegen = 0f;
            Current.m_playerAgent = null;
            Current.m_hudGlass = null;
            Current.Damage = null;
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

            if (airAmount <= config.DamageThreshold)
            {
                damageTick += Time.deltaTime;
                if (damageTick > config.DamageTime)
                {
                    AirDamage();
                }
            }

            else // airAmount > config.DamageThreshold
            {
                tickUntilRegenHealth += Time.deltaTime;
                if (healthRegenTick > healthRegenDelay)
                {
                    RegenHealth();
                }
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
            float health = Damage.Health;

            float damageAmount = config.DamageAmount;

            if(damageAmount > health)
            {
                damageAmount = health > 0.0f ? health - 0.001f : 0.0f; 
            }

            Damage.NoAirDamage(damageAmount);

            if (config.ShatterGlass)
            {
                glassShatterAmount += config.ShatterAmount;
                m_hudGlass.SetGlassShatterProgression(glassShatterAmount); 
            }
                
            damageTick = 0f;
            tickUntilRegenHealth = 0f;
            healthToRegen += damageAmount;
        }

        public void RegenHealth()
        {
            if (healthToRegen <= 0.0f) return;

            tickUntilRegenHealth = healthRegenDelay;

            healthRegenTick += Time.deltaTime;
            if(healthRegenTick > 0.25f) 
            {
                float regenAmount = config.DamageAmount;
                if (regenAmount >= healthToRegen)
                {
                    regenAmount = healthToRegen;
                    healthToRegen = 0.0f;
                }
                else
                {
                    healthToRegen -= regenAmount;
                }

                Damage.AddHealth(healthToRegen, m_playerAgent);

                healthRegenTick = 0.0f;
            }
        }

        public void UpdateAirConfig(uint fogsetting, bool LiveEditForceUpdate = false)
        {
            if (fogsetting == 0u) return;

            if (fogsetting == this.fogSetting)
            {
                if(!LiveEditForceUpdate) return;
            }

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
        
            if(GameStateManager.IsInExpedition)
            {
                AirBar.Current.UpdateAirText(config);
            }
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

            if(m_playerAgent != null && m_playerAgent.Sound != null)
                m_playerAgent.Sound.Post(EVENTS.INFECTION_EFFECT_LOOP_STOP);

            m_isInInfectionLoop = false;
        }

    }
}