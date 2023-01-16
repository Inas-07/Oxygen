using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Oxygen.Components;
using Oxygen.Utils;
using System.Collections.Generic;
using GTFO.API.Utilities;
using System.IO;


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

        //private static OxygenConfig oxygenConfig;
        public static readonly string OXYGEN_CONFIG_PATH = Path.Combine(MTFO.Managers.ConfigManager.CustomPath, "Oxygen");
        public static Dictionary<uint, OxygenBlock> lookup = new();
        private static LiveEditListener listener = null;

        // TODO: add partial data esque live edit support
        private static void LoadConfig()
        {
            foreach(string config_file in Directory.EnumerateFiles(OXYGEN_CONFIG_PATH, "*.json", SearchOption.AllDirectories))
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
        }

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

            LoadConfig();

            listener = LiveEdit.CreateListener(OXYGEN_CONFIG_PATH, "*.json", includeSubDir: true);
            listener.FileChanged += Listener_FileChanged1;
        }

        private static void Listener_FileChanged1(LiveEditEventArgs e)
        {
            Utils.Log.Warning($"LiveEdit File Changed: {e.FullPath}.");

            LiveEdit.TryReadFileContent(e.FullPath, (content) =>
            {
                OxygenConfig oxygenConfig = System.Text.Json.JsonSerializer.Deserialize<OxygenConfig>(content, ConfigManager.s_SerializerOptions);
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

