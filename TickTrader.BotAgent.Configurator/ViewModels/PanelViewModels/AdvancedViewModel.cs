using System.Collections.Generic;

namespace TickTrader.BotAgent.Configurator
{
    public class AdvancedViewModel
    {
        private ConfigurationProperies _settings;
        private RefreshManager _refreshManager;

        public AdvancedViewModel(ConfigurationProperies settings, RefreshManager _refManager = null)
        {
            _settings = settings;
            _refreshManager = _refManager;

            InitialSelectedPath = _settings.MultipleAgentProvider.BotAgentPath;
            OldValue = InitialSelectedPath;
        }

        public string InitialSelectedPath { get; }

        public string OldValue { get; }

        public List<string> AgentPaths => _settings.MultipleAgentProvider.BotAgentPaths;

        public string SelectPath
        {
            get => _settings.MultipleAgentProvider.BotAgentPath;

            set
            {
                if (_settings.MultipleAgentProvider.BotAgentPath == value)
                    return;

                _settings.MultipleAgentProvider.BotAgentPath = value;
                _refreshManager?.Refresh();
            }
        }

        public string ModelDescription { get; set; }
    }
}
