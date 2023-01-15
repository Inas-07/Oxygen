using System.IO;
using System.Text.Json;
using MTFO.Utilities;
using System.Collections.Generic;
using Newtonsoft.Json.Utilities;

namespace Oxygen.Utils
{
    public class ConfigManager
    {
        public static readonly JsonSerializerOptions s_SerializerOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            PropertyNameCaseInsensitive = true
        };

        public static void Load<T>(string file, out T config) where T : new()
        {
            if (file.Length < ".json".Length)
            {
                config = default(T);
                return;
            }
                //string filePath = Path.Combine(MTFO.Managers.ConfigManager.CustomPath, $"{file}.json");
            if (file.Substring(file.Length - ".json".Length) != ".json")
            {
                file += ".json";
            }

            string filePath = Path.Combine(MTFO.Managers.ConfigManager.CustomPath, "Oxygen", file);

            file = File.ReadAllText(filePath);
            config = System.Text.Json.JsonSerializer.Deserialize<T>(file, s_SerializerOptions);
        }
    }
    
    public class OxygenConfig
    {
        public List<OxygenBlock> Blocks { get; set; } = new() { new() };
    }

    public class OxygenBlock
    {
        public List<uint> LevelLayouts { get; set; } = new() { 0U };
        public float AirLoss { get; set; } = 0.0f;
        public float AirGain { get; set; } = 0.0f;
        public float DamageTime { get; set; } = 0.0f; 
        public float DamageAmount { get; set; } = 0.0f;
        public bool ShatterGlass { get; set; } = false;
        public float ShatterAmount { get; set; } = 0.0f;
        public float DamageThreshold { get; set; } = 0.1f;
        public bool AlwaysDisplayAirBar { get; set; } = true;
    }
}