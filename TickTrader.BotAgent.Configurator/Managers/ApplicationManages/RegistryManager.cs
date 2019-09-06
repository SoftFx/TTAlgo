using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TickTrader.Algo.Common.Lib;

namespace TickTrader.BotAgent.Configurator
{
    public class RegistryManager
    {
        private readonly RegistryKey _base64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

        public const string AgentPathNameProperty = "AgentConfigurationPaths";

        public List<RegistryNode> AgentNodes { get; }

        public RegistryNode CurrentAgent { get; private set; }

        public RegistryNode OldAgent { get; private set; }

        public RegistryManager(string registryApplicationName, string appSettings, bool developer, string exeName)
        {
            AgentNodes = new List<RegistryNode>();

            var agentFolder = _base64.OpenSubKey(Path.Combine("SOFTWARE", registryApplicationName));
            var agentPath = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName;

            foreach (var agentName in agentFolder.GetSubKeyNames())
            {
                var node = new RegistryNode(agentFolder.OpenSubKey(agentName), appSettings, developer, exeName);

                if (agentPath == node.Path)
                    CurrentAgent = node;

                AgentNodes.Add(node);
            }

            if (CurrentAgent == null)
                CurrentAgent = AgentNodes.Count > 0 ? AgentNodes[0] : throw new Exception("Please, install TickTrader BotAgent!");

            OldAgent = CurrentAgent;
        }

        public void ChangeCurrentAgent(string path)
        {
            if (path == null)
                return;

            CurrentAgent = AgentNodes.Find(n => n.Path == path);
            OldAgent = CurrentAgent;
        }
    }

    public class RegistryNode
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public string FullVersion => IsDeveloper ? $"{Version} ({BuildDate}) Developer Version" : $"{Version} ({BuildDate})";

        public string AppSettingPath { get; }

        public string Path { get; }

        public string ServiceId { get; }

        public bool IsDeveloper { get; }

        public string Version { get; private set; }

        public string ExePath { get; private set; }

        public string BuildDate { get; private set; } = DateTime.MinValue.ToString("yyyy.MM.dd");


        public RegistryNode(RegistryKey key, string appSetting, bool developer, string exeName)
        {
            Path = key.GetValue(nameof(Path)).ToString();
            ServiceId = key.GetValue(nameof(ServiceId)).ToString();
            Version = key.GetValue(nameof(Version)).ToString();
            IsDeveloper = developer;

            AppSettingPath = System.IO.Path.Combine(Path, appSetting);

            GetBuildDate(exeName);
        }

        private void GetBuildDate(string exeName)
        {
            ExePath = System.IO.Path.Combine(Path, $"{exeName}.exe");

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
