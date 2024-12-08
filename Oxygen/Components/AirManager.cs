using UnityEngine;
using System;
using Player;
using AK;
using GameData;
using Oxygen.Config;
using GTFO.API;
using Oxygen.Utils;
using static GameData.GD;
using SNetwork;
using Il2CppInterop.Runtime.Injection;

namespace Oxygen.Components
{
    // AirManager doesn't handle environment state change.
    // We need to tell the AirManager about the change in Patches
    // Controls air bar
    public class AirManager : MonoBehaviour
    {
        public PlayerAgent m_playerAgent;

        private HUDGlassShatter m_hudGlass;
        private Dam_PlayerDamageBase Damage;

        // air config
        public OxygenBlock Config { get; private set; } = null;
        private uint fogSetting = 0u;
        private FogSettingsDataBlock fogSettingDB = null;
        private float airAmount = 1f;
        private float damageTick = 0f;
        private float glassShatterAmount = 0f;
        private bool m_isInInfectionLoop = false;

        // health regen
        private bool isRegeningHealth = false;
        private float healthToRegen = 0f;
        private float healthRegenTick = 0.0f;
        
        private float tickUntilHealthRegenHealthStart = 0.0f;
        
        private readonly float regenHealthTickInterval = 0.25f;

        private float healthRegenAmountPerInterval = 0.0f;

        internal bool PlayerShouldCough = false;
        private readonly float CoughPerLoss = 0.1f;
        private float CoughLoss = 0.0f;

        public AirManager(IntPtr value) : base(value) { }

        internal void Setup()
        {
            m_playerAgent = gameObject.GetComponent<PlayerAgent>();

            LevelAPI.OnBuildDone += SetupAirManager;
            LevelAPI.OnEnterLevel += SetupAirManager;
            LevelAPI.OnBuildStart += Clear;
            LevelAPI.OnLevelCleanup += Clear;

            OxygenLogger.Warning($"GameState: {GameStateManager.CurrentStateName}");
            if (GameStateManager.CurrentStateName == eGameStateName.ExpeditionFail) // checkpoint reload
            {
                SetupAirManager();
                AirBar.Setup();
                AirPlane.Setup();
            }
        }
        
        internal void SetupAirManager()
        {
            m_hudGlass = m_playerAgent.FPSCamera.GetComponent<HUDGlassShatter>();
            Damage = m_playerAgent.gameObject.GetComponent<Dam_PlayerDamageBase>();

            UpdateAirConfig(RundownManager.ActiveExpedition.Expedition.FogSettings);
        }

        private void OnDestroy()
        {
            Clear();
            LevelAPI.OnBuildDone -= SetupAirManager;
            LevelAPI.OnBuildStart -= Clear;
            LevelAPI.OnLevelCleanup -= Clear;
        }


        internal void Clear()
        {
            if (m_isInInfectionLoop)
            {
                StopInfectionLoop();
            }

            Config = null;
            fogSetting = 0u;
            fogSettingDB = null;
            airAmount = 1f;
            damageTick = 0f;
            glassShatterAmount = 0f;
            healthToRegen = 0f;
            m_hudGlass = null;
            Damage = null;
        }

        private void Update()
        {
            if (!RundownManager.ExpeditionIsStarted) return;
            if (!HasConfig)
            {
                AirBar.Current.SetVisible(false);
                return;
            }

            // Breathing intensity, Coughing, and Damage Tick
            if (airAmount == 1f)
            {
                if(Config.AlwaysDisplayAirBar)
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

            if (airAmount <= Config.DamageThreshold)
            {
                damageTick += Time.deltaTime;
                if (damageTick > Config.DamageTime)
                {
                    if(m_playerAgent.Alive)
                    {
                        AirDamage();
                    }
                }

                isRegeningHealth = false;
            }

            else if(healthToRegen > 0.0f)// airAmount > config.DamageThreshold
            {
                tickUntilHealthRegenHealthStart += Time.deltaTime;

                //if(tickUntilHealthRegenHealthStart <= healthRegenDelay)
                //    Log.Debug($"Waiting for health regen. Current tick {tickUntilHealthRegenHealthStart}");
                if (tickUntilHealthRegenHealthStart > Config.TimeToStartHealthRegen)
                {
                    if(healthRegenAmountPerInterval == 0.0f)
                    {
                        healthRegenAmountPerInterval = healthToRegen * (regenHealthTickInterval / Config.TimeToCompleteHealthRegen);
                    }

                    RegenHealth();

                    if(isRegeningHealth == false)
                    {
                        Damage.m_nextRegen = Clock.Time + Config.TimeToStartHealthRegen + Config.TimeToCompleteHealthRegen;
                        isRegeningHealth = true;
                    }
                }
            }

            else
            {
                isRegeningHealth = false;
            }
        }

        public void AddAir()
        {
            if (!HasConfig) return;

            float amount = this.Config.AirGain;
            airAmount = Mathf.Clamp01(airAmount + amount);
            AirBar.Current.UpdateAirBar(airAmount);

            // If fogSettingDB.Infection > 0, infection effect sound plays via vanilla code.
            var fogsetting = FogSettingDB;
            if (fogsetting != null)
            {
                if (fogsetting.Infection <= 0.0f && this.m_isInInfectionLoop)
                {
                    StopInfectionLoop();
                }
            }
        }

        public void RemoveAir(float amount) 
        {
            if (!HasConfig) return;

            //airAmount = Mathf.Clamp01(airAmount - amount);
            // `amount` doesn't update when using LiveEdit 
            // so I changed to config.amount
            amount = this.Config.AirLoss;

            airAmount = Mathf.Clamp01(airAmount - amount);
            AirBar.Current?.UpdateAirBar(airAmount);

            // If fogSettingDB.Infection > 0, infection effect sound plays via vanilla code.
            var fogsetting = FogSettingDB;
            if (fogsetting != null)
            {
                if (fogsetting.Infection <= 0.0f && amount > 0.0f)
                {
                    StartInfectionLoop();
                }
            }
        }

        internal void AirDamage()
        {
            float health = Damage.Health;

            float damageAmount = Config.DamageAmount;
            Damage.m_nextRegen = Clock.Time + Config.TimeToStartHealthRegen; // TODO: test this

            if (health <= 1.0f) return; // 4% health in game
            //if(damageAmount > health)
            //{
            //    damageAmount = health > 0.0f ? health - 0.001f : 0.0f; 
            //}

            Damage.NoAirDamage(damageAmount);

            if (Config.ShatterGlass)
            {
                glassShatterAmount += Config.ShatterAmount;
                m_hudGlass.SetGlassShatterProgression(glassShatterAmount); 
            }
                
            damageTick = 0f;
            tickUntilHealthRegenHealthStart = 0f;
            healthRegenAmountPerInterval = 0.0f;
            healthToRegen += damageAmount * Config.HealthRegenProportion;
            CoughLoss += damageAmount;
            if (CoughLoss > CoughPerLoss)
            {
                PlayerShouldCough = true;
                CoughLoss = 0.0f;
            }
            //Log.Debug($"AirDamage: healthToRegen {healthToRegen}, reset delay tick");
        }

        internal void RegenHealth()
        {
            if (healthToRegen <= 0.0f) return;

            tickUntilHealthRegenHealthStart = Config.TimeToStartHealthRegen;

            healthRegenTick += Time.deltaTime;
            if(healthRegenTick > regenHealthTickInterval) 
            {
                float regenAmount = healthRegenAmountPerInterval;

                if (regenAmount >= healthToRegen)
                {
                    regenAmount = healthToRegen;
                    healthToRegen = 0.0f;
                    tickUntilHealthRegenHealthStart = 0.0f;
                    healthRegenAmountPerInterval = 0.0f;
                    isRegeningHealth = false;
                }
                else
                {
                    healthToRegen -= regenAmount;
                }

                Damage.AddHealth(regenAmount, m_playerAgent);
                //Log.Debug($"Regen: amount {regenAmount}");

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
                this.Config = Plugin.lookup[fogsetting];
            }
            else if (Plugin.lookup.ContainsKey(0U))
            {
                this.Config = Plugin.lookup[0U];
            }
            else
            {
                this.Config = null;
                airAmount = 1.0f; // no air config. reset air amount
            }

            this.fogSetting = fogsetting;
            this.fogSettingDB = FogSettingsDataBlock.GetBlock(fogsetting);
        
            //if(GameStateManager.IsInExpedition)
            //{
                AirBar.Current.UpdateAirText(Config);
            //}
        }

        internal void ResetHealthToRegen()
        {
            healthRegenTick = 0.0f;
            healthToRegen = 0.0f;
            tickUntilHealthRegenHealthStart = 0.0f;
            //Log.Warning("Reset health to regen");
        }

        public uint FogSetting
        {
            get { 
                int i = EnvironmentStateManager.Current.m_latestKnownLocalDimensionCreationIndex;
                if (i < 0 || i >= EnvironmentStateManager.Current.m_stateReplicator.State.FogStates.Count) return 0;
                return EnvironmentStateManager.Current.m_stateReplicator.State.FogStates[i].FogDataID;
            }
        }

        public FogSettingsDataBlock? FogSettingDB
        {
            get
            {
                if (FogSetting == 0) return null;
                return GameDataBlockBase<FogSettingsDataBlock>.GetBlock(FogSetting);
            }
        }

        public float AirLoss() => Config == null ? 0f : Config.AirLoss;

        public bool AlwaysDisplayAirBar() => Config == null ? false : Config.AlwaysDisplayAirBar;

        public float HealthToRegen() => healthToRegen;

        public string AirText() => Config == null ? null : Config.AirText.Text;

        public float AirTextX() => Config == null ? 0.0f : Config.AirText.x;

        public float AirTextY() => Config == null ? 0.0f : Config.AirText.y;

        public bool HasConfig => Config != null;

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

        public static AirManager? Current => SNet.HasLocalPlayer && SNet.LocalPlayer.HasPlayerAgent ? SNet.LocalPlayer.PlayerAgent.Cast<PlayerAgent>().GetComponent<AirManager>() : null; 
    
        static AirManager()
        {
            ClassInjector.RegisterTypeInIl2Cpp<AirManager>();
        }
    }
}