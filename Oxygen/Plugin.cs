using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Oxygen.Components;
using Oxygen.Utils;
using System.Collections.Generic;

namespace Oxygen
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    [BepInProcess("GTFO.exe")]
    [BepInDependency(MTFO.MTFO.GUID, BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BasePlugin
    {
        public const string
            MODNAME = "Oxygen",
            AUTHOR = "chasetug",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "1.0.0";
        
        public static OxygenConfig oxygenConfig;
        public static Dictionary<uint, OxygenBlock> lookup = new();

        public override void Load()
        {
            ClassInjector.RegisterTypeInIl2Cpp<AirManager>();
            RundownManager.add_OnExpeditionGameplayStarted((Action) AirManager.Setup);

            ClassInjector.RegisterTypeInIl2Cpp<AirBar>();
            RundownManager.add_OnExpeditionGameplayStarted((Action) AirBar.Setup);

            ClassInjector.RegisterTypeInIl2Cpp<AirPlane>();
            RundownManager.add_OnExpeditionGameplayStarted((Action) AirPlane.Setup);

            var harmony = new Harmony(GUID);
            harmony.PatchAll();

            ConfigManager.Load("oxygen", out oxygenConfig);
            foreach (OxygenBlock block in oxygenConfig.Blocks)
            {
                foreach (uint id in block.LevelLayouts)
                {   
                    if (!lookup.ContainsKey(id))
                    {
                        lookup.Add(id, block);
                    }
                }
            }
        }
    }
}

