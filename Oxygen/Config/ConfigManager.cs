using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using Oxygen.Utils.PartialData;
using System.Text.Json.Serialization;
using Oxygen.Utils;

namespace Oxygen.Config
{
    public class ConfigManager
    {
        private static readonly JsonSerializerOptions s_SerializerOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, s_SerializerOptions);
        }

        public static string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, s_SerializerOptions);
        }

        static ConfigManager()
        {
            s_SerializerOptions.Converters.Add(new JsonStringEnumConverter());

            if (MTFOPartialDataUtil.IsLoaded && MTFOPartialDataUtil.Initialized)
            {
                s_SerializerOptions.Converters.Add(MTFOPartialDataUtil.PersistentIDConverter);
                s_SerializerOptions.Converters.Add(MTFOPartialDataUtil.LocalizedTextConverter);
                Log.Message("PartialData Support Found!");
            }
            else
            {
                // handle localized text, i.e. `AirText`
                s_SerializerOptions.Converters.Add(new LocalizedTextConverter());
            }
        }

        public static void Load<T>(string file, out T config) where T : new()
        {
            if (file.Length < ".json".Length)
            {
                config = default;
                return;
            }
            //string filePath = Path.Combine(MTFO.Managers.ConfigManager.CustomPath, $"{file}.json");
            if (file.Substring(file.Length - ".json".Length) != ".json")
            {
                file += ".json";
            }

            string filePath = Path.Combine(MTFO.Managers.ConfigManager.CustomPath, "Oxygen", file);

            file = File.ReadAllText(filePath);
            config = Deserialize<T>(file);
        }
    }
}