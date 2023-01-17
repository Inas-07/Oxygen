using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Oxygen.Components;
using System.Collections.Generic;
using GTFO.API.Utilities;
using GTFO.API;
using System.IO;
using Oxygen.Config;

namespace Oxygen
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    [BepInProcess("GTFO.exe")]
    [BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(MTFO.MTFO.GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(Utils.PartialData.MTFOUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(Utils.PartialData.MTFOPartialDataUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]

    public class Plugin : BasePlugin
    {
        public const string
            MODNAME = "Oxygen",
            AUTHOR = "chasetug",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "1.0.1";

        //private static OxygenConfig oxygenConfig;
        public static readonly string OXYGEN_CONFIG_PATH = Path.Combine(MTFO.Managers.ConfigManager.CustomPath, "Oxygen");
        public static Dictionary<uint, OxygenBlock> lookup = new();
        private static LiveEditListener listener = null;

        public override void Load()
        {
            ClassInjector.RegisterTypeInIl2Cpp<AirManager>();
            LevelAPI.OnBuildStart += AirManager.Setup;
            //RundownManager.add_OnExpeditionGameplayStarted((Action) AirManager.Setup);

            ClassInjector.RegisterTypeInIl2Cpp<AirBar>();
            LevelAPI.OnBuildStart += AirBar.Setup;
            //RundownManager.add_OnExpeditionGameplayStarted((Action) AirBar.Setup);

            ClassInjector.RegisterTypeInIl2Cpp<AirPlane>();
            LevelAPI.OnBuildStart += AirPlane.Setup;
            //RundownManager.add_OnExpeditionGameplayStarted((Action) AirPlane.Setup);

            var harmony = new Harmony(GUID);
            harmony.PatchAll();

            foreach (string config_file in Directory.EnumerateFiles(OXYGEN_CONFIG_PATH, "*.json", SearchOption.AllDirectories))
            {
                OxygenConfig oxygenConfig;
                ConfigManager.Load(config_file, out oxygenConfig);
                foreach (OxygenBlock block in oxygenConfig.Blocks)
                {
                    foreach (uint id in block.FogSettings)
                    {
                        if (!lookup.ContainsKey(id))
                        {
                            lookup.Add(id, block);
                        }
                    }
                }
            }

            listener = LiveEdit.CreateListener(OXYGEN_CONFIG_PATH, "*.json", includeSubDir: true);
            listener.FileChanged += Listener_FileChanged1;
        }

        private static void Listener_FileChanged1(LiveEditEventArgs e)
        {
            Utils.Log.Warning($"LiveEdit File Changed: {e.FullPath}.");

            LiveEdit.TryReadFileContent(e.FullPath, (content) =>
            {
                OxygenConfig oxygenConfig = ConfigManager.Deserialize<OxygenConfig>(content);
                foreach (OxygenBlock block in oxygenConfig.Blocks)
                {
                    foreach (uint id in block.FogSettings)
                    {
                        if (lookup.ContainsKey(id))
                        {
                            lookup.Remove(id);
                        }
                        lookup.Add(id, block);
                    }
                }

                if (GameStateManager.IsInExpedition)
                {
                    AirManager.Current.UpdateAirConfig(AirManager.Current.FogSetting());
                }
            });
        }
    }
}

