using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;

namespace TickTrader.BotAgent.Configurator
{
    public class RegistryManager
    {
        private readonly RegistryKey _base64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

        public const string AgentPathNameProperty = "AgentConfigurationPaths";

        public List<RegistryNode> AgentNodes { get; }

        public RegistryNode CurrentAgent { get; private set; }

        public RegistryNode OldAgent { get; private set; }

        public RegistryManager(string registryApplicationName, string appSettings)
        {
            AgentNodes = new List<RegistryNode>();

            var agentFolder = _base64.OpenSubKey(Path.Combine("SOFTWARE", registryApplicationName));

            foreach (var agentName in agentFolder.GetSubKeyNames())
            {
                var node = new RegistryNode(agentFolder.OpenSubKey(agentName), appSettings);

                if (Environment.CurrentDirectory.Contains(node.Path))
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
        public string Path { get; }

        public string ServiceId { get; }

        public string Version { get; }

        public string AppSettingPath { get; }

        public RegistryNode(RegistryKey key, string appSetting)
        {
            Path = key.GetValue(nameof(Path)).ToString();
            ServiceId = key.GetValue(nameof(ServiceId)).ToString();
            Version = key.GetValue(nameof(Version)).ToString();

            AppSettingPath = System.IO.Path.Combine(Path, appSetting);
        }
    }
}
