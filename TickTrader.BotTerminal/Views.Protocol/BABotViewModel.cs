using Caliburn.Micro;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class BABotViewModel : PropertyChangedBase
    {
        private BotModelInfo _entity;


        public string InstanceId => _entity.InstanceId;

        public BotStates State => _entity.State;

        public string AccountKey { get; }


        public BABotViewModel(BotModelInfo entity, BotAgentModel botAgent)
        {
            _entity = entity;
            AccountKey = BotAgentModel.GetAccountKey(_entity.Account);

            botAgent.BotStateChanged += BotAgentOnBotStateChanged;
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
