using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TickTrader.Algo.Common.Lib;
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


        public RegistryManager(List<string> registryApplicationNames, string appSettings, bool developer)
        {
            ServerNodes = new List<RegistryNode>();

            var serverPath = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName;

            foreach (var registryName in registryApplicationNames)
            {
                var agentFolder = _base64.OpenSubKey(Path.Combine("SOFTWARE", "TickTrader", registryName));

                foreach (var agentName in agentFolder.GetSubKeyNames())
                {
                    var node = new RegistryNode(agentFolder.OpenSubKey(agentName), appSettings, developer, registryName);

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

        public string FullVersionWithDeveloper => IsDeveloper ? $"{FullVersion} Developer Version" : FullVersion;

        public string AppSettingPath { get; }

        public string FolderPath { get; }

        public string NodeName { get; }

        public string ServiceId { get; }

        public bool IsDeveloper { get; }

        public string Version { get; private set; }

        public string ExePath { get; private set; }

        public string BuildDate { get; private set; } = DateTime.MinValue.ToString("yyyy.MM.dd");


        public RegistryNode(RegistryKey key, string appSetting, bool developer, string typeName)
        {
            NodeName = typeName;
            FolderPath = key.GetValue("Path").ToString();
            ServiceId = key.GetValue(nameof(ServiceId)).ToString();
            Version = key.GetValue(nameof(Version)).ToString();
            IsDeveloper = developer;

            AppSettingPath = Path.Combine(FolderPath, appSetting);
            ExePath = Path.Combine(FolderPath, $"TickTrader.{NodeName}.exe");

            GetBuildDate(typeName);
        }

        private void GetBuildDate(string exeName)
        {
            try
            {
                var assemblyName = AssemblyName.GetAssemblyName(ExePath);
                Version = assemblyName.Version.ToString();
                BuildDate = assemblyName.GetLinkerTime().ToString("yyyy.MM.dd");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }
    }
}
