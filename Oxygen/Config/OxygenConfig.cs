using System;
using System.Collections.Generic;
using GameData;
using System.Text;
using Localization;

namespace Oxygen.Config
{
    public class AirText
    {
        public float x { set; get; } = 0f;
        public float y { set; get; } = 0f;
        public LocalizedText Text { get; set; } = null;
    }

    public class OxygenConfig
    {
        public List<OxygenBlock> Blocks { get; set; } = new() { new() };
    }

    public class OxygenBlock
    {
        // Don't change default value: it matters when in-level fog setting changed.
        public float AirLoss { get; set; } = 0.0f;
        public float AirGain { get; set; } = 1.0f;
        public float DamageTime { get; set; } = 1.0f;
        public float DamageAmount { get; set; } = 0.0f;
        public bool ShatterGlass { get; set; } = false;
        public float ShatterAmount { get; set; } = 0.0f;
        public float DamageThreshold { get; set; } = 0.1f;
        public bool AlwaysDisplayAirBar { get; set; } = false;
        public float HealthRegenProportion { get; set; } = 1.0f;
        public float TimeToStartHealthRegen { get;set; } = 3.0f;
        public float TimeToCompleteHealthRegen { get; set; } = 5.0f;
        public AirText AirText { set; get; } = null;
        public List<uint> FogSettings { get; set; } = new() { 0U };
    }
}
