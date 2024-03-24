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
            AUTHOR = "Inas",
            GUID = AUTHOR + "." + MODNAME,
            VERSION = "1.3.3";

        //private static OxygenConfig oxygenConfig;
        public static readonly string OXYGEN_CONFIG_PATH = Path.Combine(MTFO.Managers.ConfigManager.CustomPath, "Oxygen");
        public static Dictionary<uint, OxygenBlock> lookup = new();
        private static LiveEditListener listener = null;

        public override void Load()
        {
            if (!Directory.Exists(OXYGEN_CONFIG_PATH))
            {
                Directory.CreateDirectory(OXYGEN_CONFIG_PATH);
                var file = File.CreateText(Path.Combine(OXYGEN_CONFIG_PATH, "Template.json"));
                file.WriteLine(ConfigManager.Serialize(new OxygenConfig()));
                file.Flush();
                file.Close();
            }

            ClassInjector.RegisterTypeInIl2Cpp<AirManager>();
            //LevelAPI.OnBuildStart += AirManager.Setup;
            LevelAPI.OnBuildDone += AirManager.OnBuildDone;
            LevelAPI.OnLevelCleanup += AirManager.OnLevelCleanup;

            ClassInjector.RegisterTypeInIl2Cpp<AirBar>();
            LevelAPI.OnBuildStart += AirBar.Setup;
            LevelAPI.OnLevelCleanup += AirBar.OnLevelCleanup; 

            ClassInjector.RegisterTypeInIl2Cpp<AirPlane>();
            LevelAPI.OnBuildStart += AirPlane.OnBuildStart;
            LevelAPI.OnLevelCleanup += AirPlane.OnLevelCleanup;
            
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
            Utils.OxygenLogger.Warning($"LiveEdit File Changed: {e.FullPath}.");

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
                        Utils.OxygenLogger.Warning($"Replaced OxygenConfig for FogSetting: {id}.");
                    }
                }

                if (GameStateManager.IsInExpedition)
                {
                    AirManager.Current.UpdateAirConfig(AirManager.Current.FogSetting(), true);
                }
            });
        }
    }
}

