using System.IO;
using System.Text.Json;
using MTFO.Utilities;
using System.Collections.Generic;


namespace Oxygen.Utils
{
    public class ConfigManager
    {
        internal static readonly JsonSerializerOptions s_SerializerOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            PropertyNameCaseInsensitive = true
        };

        public static void Load<T>(string file, out T config) where T : new()
        {
            string filePath = Path.Combine(MTFO.Managers.ConfigManager.CustomPath, $"{file}.json");
            if (PathUtil.CheckCustomFile($"{file}.json", out string path))
            {
                file = File.ReadAllText(filePath);
                config = System.Text.Json.JsonSerializer.Deserialize<T>(file, s_SerializerOptions);
            }
            else
            {
                config = new();
                file = System.Text.Json.JsonSerializer.Serialize(config);
                File.WriteAllText(filePath, file);
            }
        }
    }
    
    public class OxygenConfig
    {
        public List<OxygenBlock> Blocks { get; set; } = new() { new() };
    }

    public class OxygenBlock
    {
        public List<uint> LevelLayouts { get; set; } = new() { 0U };
        public float AirLoss { get; set; } = 0;
        public float AirGain { get; set; } = 0;
        public float DamageTime { get; set; } = 0; 
        public float DamageAmount { get; set; } = 0;
        public bool ShatterGlass { get; set; } = false;
        public float ShatterAmount { get; set; } = 0;
    }
}