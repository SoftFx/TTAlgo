﻿using Caliburn.Micro;
using TickTrader.Algo.Common.Info;

namespace TickTrader.BotTerminal
{
    internal class AlgoBotViewModel : PropertyChangedBase
    {
        public BotModelInfo Info { get; }

        public AlgoAgentViewModel Agent { get; }


        public string InstanceId => Info.InstanceId;

        public AccountKey Account => Info.Account;

        public BotStates State => Info.State;


        public AlgoBotViewModel(BotModelInfo info, AlgoAgentViewModel agent)
        {
            Info = info;
            Agent = agent;

            Agent.Model.BotStateChanged += OnBotStateChanged;
        }


        public void Start()
        {
            Agent.StartBot(Info.InstanceId).Forget();
        }

        public void Stop()
        {
            Agent.StopBot(Info.InstanceId).Forget();
        }

        public void Remove()
        {
            Agent.RemoveBot(Info.InstanceId).Forget();
        }

        public void OpenSettings()
        {
        }


        private void OnBotStateChanged(BotModelInfo bot)
        {
            if (bot.InstanceId == Info.InstanceId)
            {
                NotifyOfPropertyChange(nameof(State));
            }
        }
    }
}
