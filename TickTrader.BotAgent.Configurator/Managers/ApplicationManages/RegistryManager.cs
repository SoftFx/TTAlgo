using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TickTrader.Algo.AppCommon;
using TickTrader.Algo.Core.Lib;
using TickTrader.BotAgent.Configurator.Properties;

namespace TickTrader.BotAgent.Configurator
{
    public class RegistryManager
    {
        private readonly RegistryKey _base64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

        public const string AgentPathNameProperty = "AgentConfigurationPaths";

        public List<RegistryNode> ServerNodes { get; }

        public RegistryNode CurrentServer { get; private set; }

        public RegistryNode OldServer { get; private set; }


        public RegistryManager()
        {
            ServerNodes = new List<RegistryNode>();

            var serverPath = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName;

            foreach (var pair in ConfigurationProperies.RegistryApplicationNames)
            {
                var agentFolder = _base64.OpenSubKey(Path.Combine("SOFTWARE", "TickTrader", pair.Type));

                if (agentFolder != null)
                    foreach (var agentName in agentFolder.GetSubKeyNames())
                    {
                        var node = new RegistryNode(agentFolder.OpenSubKey(agentName), ConfigurationProperies.AppSettings, pair);

                        if (serverPath == node.FolderPath)
                            CurrentServer = node;

                        ServerNodes.Add(node);
                    }
            }

            if (CurrentServer == null)
                CurrentServer = ServerNodes.Count > 0 ? ServerNodes[0] : throw new Exception(Resources.AgentNotFoundEx);

            OldServer = CurrentServer;
        }

        public void ChangeCurrentAgent(string path)
        {
            if (path == null)
                return;

            CurrentServer = ServerNodes.Find(n => n.FolderPath == path);
            OldServer = CurrentServer;
        }
    }

    public class RegistryNode
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();


        public string FullVersion => $"{Version} ({BuildDate})";

        public string BackupArchiveName => $"{ServiceId} {Version} backup {DateTime.UtcNow:dd.MM.yyyy HH.mm.ss}.zip";


        public string AppSettingPath { get; }

        public string LogsFilePath { get; }

        public string FolderPath { get; }

        public string NodeName { get; }

        public string ServiceId { get; }

        public string Version { get; private set; }

        public string ExePath { get; private set; }

        public string BuildDate { get; private set; } = DateTime.MinValue.ToString("yyyy.MM.dd");


        public RegistryNode(RegistryKey key, string appSetting, (string Name, string LogFile) settings)
        {
            try
            {
                NodeName = settings.Name;

                FolderPath = key.GetValue("Path").ToString();
                LogsFilePath = Path.Combine(FolderPath, "Logs", settings.LogFile);

                ServiceId = key.GetValue(nameof(ServiceId)).ToString();
                Version = key.GetValue(nameof(Version)).ToString();

                AppSettingPath = Path.Combine(FolderPath, appSetting);
                var binFolder = Path.Combine(FolderPath, "bin", "current");
                if (!Directory.Exists(binFolder))
                    binFolder = FolderPath; // before 1.24
                ExePath = Path.Combine(binFolder, $"TickTrader.{NodeName}.exe");

                var assemblyPath = Path.Combine(binFolder, $"TickTrader.{NodeName}.dll");
                if (!File.Exists(assemblyPath))
                    assemblyPath = ExePath; // .Net Framework, before 1.19

                if (!File.Exists(assemblyPath))
                {
                    _logger.Error($"Resolved main module path doesn't exists: '{assemblyPath}'");
                }
                else
                {
                    var versionInfo = AppVersionInfo.GetFromDllPath(assemblyPath);
                    Version = versionInfo.Version;
                    BuildDate = versionInfo.BuildDate;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }
    }
}
