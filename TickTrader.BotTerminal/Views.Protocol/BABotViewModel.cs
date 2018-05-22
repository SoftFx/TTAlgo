using Caliburn.Micro;
using TickTrader.Algo.Core;
using TickTrader.Algo.Protocol.Sfx;

namespace TickTrader.BotTerminal
{
    internal class BABotViewModel : PropertyChangedBase
    {
        private BotModelEntity _entity;


        public string InstanceId => _entity.InstanceId;

        public BotState State => _entity.State;

        public string AccountKey { get; }


        public BABotViewModel(BotModelEntity entity, BotAgentModel botAgent)
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
