using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace TickTrader.Algo.AppCommon
{
    public class AppAccessInfo
    {
        public const string DataFileName = "access-info.json";

        private static readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };


        public Dictionary<string, string> AccessByPath { get; set; } = new Dictionary<string, string>();


        public static Exception AddAccessRecord(string dataPath)
        {
            try
            {
                var filePath = Path.Combine(dataPath, DataFileName);
                var data = File.Exists(filePath) ? LoadFromJson(filePath) : new AppAccessInfo();
                var binPath = Assembly.GetEntryAssembly().Location;
                data.AccessByPath[binPath] = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss");
                data.SaveAsJson(filePath);
            }
            catch(Exception ex)
            {
                return ex;
            }
            return null;
        }

        public static AppAccessInfo LoadFromJson(string path)
        {
            using var file = File.Open(path, FileMode.Open, FileAccess.Read);
            return JsonSerializer.Deserialize<AppAccessInfo>(file, _serializerOptions);
        }

        public void SaveAsJson(string path)
        {
            using var file = File.Open(path, FileMode.Create);
            JsonSerializer.Serialize(file, this, _serializerOptions);
        }
    }
}
