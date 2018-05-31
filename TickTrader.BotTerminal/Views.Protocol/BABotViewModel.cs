using Caliburn.Micro;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class BABotViewModel : PropertyChangedBase
    {
        private BotModelInfo _entity;
        private RemoteAgent _remoteAgent;


        public string InstanceId => _entity.InstanceId;

        public AccountKey Account => _entity.Account;

        public BotStates State => _entity.State;


        public BABotViewModel(BotModelInfo entity, RemoteAgent remoteAgent)
        {
            _entity = entity;
            _remoteAgent = remoteAgent;

            _remoteAgent.BotAgent.BotStateChanged += BotAgentOnBotStateChanged;
        }


        public void Start()
        {
            _remoteAgent.StartBot(_entity.InstanceId);
        }

        public void Stop()
        {
            _remoteAgent.StopBot(_entity.InstanceId);
        }


        private void BotAgentOnBotStateChanged(string instanceId)
        {
            if (instanceId == _entity.InstanceId)
            {
                NotifyOfPropertyChange(nameof(State));
            }
        }
    }
}
