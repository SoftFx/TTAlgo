using System.IO;
using System.Text.Json;

namespace TickTrader.Algo.AppCommon
{
    public class AppInfo
    {
        public int CfgVersion { get; set; } = 1;

        public string InstallId { get; set; }

        public bool IsPortable { get; set; }

        public string DataPath { get; set; }


        public static AppInfo LoadFromJson(string path)
        {
            using var file = File.Open(path, FileMode.Open);
            return JsonSerializer.Deserialize<AppInfo>(file);
        }

        public void SaveAsJson(string path)
        {
            using var file = File.Open(path, FileMode.Create);
            JsonSerializer.Serialize(file, this);
        }
    }
}
