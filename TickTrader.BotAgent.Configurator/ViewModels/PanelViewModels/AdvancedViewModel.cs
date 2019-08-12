
using System.Collections.Generic;

namespace TickTrader.BotAgent.Configurator
{
    public class AdvancedViewModel : BaseViewModel
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly RefreshManager _refreshManager;
        private readonly string _keyPath;

        private ConfigurationProperies _settings;

        public AdvancedViewModel(ConfigurationProperies settings, RefreshManager _refManager = null) : base(nameof(AdvancedViewModel))
        {
            _settings = settings;
            _refreshManager = _refManager;

            InitialSelectedPath = _settings.MultipleAgentProvider.BotAgentPath;
            OldValue = InitialSelectedPath;

            _keyPath = $"{nameof(AdvancedViewModel)} {nameof(SelectPath)}";
        }

        public string InitialSelectedPath { get; }

        public string OldValue { get; set; }

        public List<string> AgentPaths => _settings.MultipleAgentProvider.BotAgentPaths;

        public string SelectPath
        {
            get => _settings.MultipleAgentProvider.BotAgentPath;

            set
            {
                if (_settings.MultipleAgentProvider.BotAgentPath == value)
                    return;

                _settings.MultipleAgentProvider.BotAgentPath = value;

                _refreshManager?.AddUpdate(_keyPath);
                _logger.Info(GetChangeMessage(_keyPath, _settings.MultipleAgentProvider.BotAgentConfigPath, value));
            }
        }

        public string ModelDescription { get; set; }
    }
}
