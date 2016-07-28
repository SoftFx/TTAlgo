using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class BotStateViewModel : Screen
    {
        public BotStateViewModel(TradeBotModel2 bot)
        {
            this.Bot = bot;
            Bot.Removed += Bot_Removed;
            Bot.StateChanged += Bot_StateChanged;
            DisplayName = "Status: " + bot.Name;
            BotName = string.Format("{0} ({1}, {2})", Bot.Name, "", "");
            Bot_StateChanged(Bot);
        }

        public TradeBotModel2 Bot { get; private set; }
        public string BotName { get; private set; }
        public string ExecStatus { get; private set; }
        public bool IsStarted { get { return Bot.State == BotModelStates.Running || Bot.State == BotModelStates.Stopping; } }
        public bool CanStartStop { get { return Bot.State == BotModelStates.Running || Bot.State == BotModelStates.Stopped; } }

        public override void TryClose(bool? dialogResult = default(bool?))
        {
            base.TryClose(dialogResult);
            Bot.StateChanged -= Bot_StateChanged;
            Bot.Removed -= Bot_Removed;
        }

        public async void StartStop()
        {
            if (Bot.State == BotModelStates.Running)
                await Bot.Stop();
            else if (Bot.State == BotModelStates.Stopped)
                await Bot.Start();
        }

        private void Bot_Removed(TradeBotModel2 bot)
        {
            TryClose();
        }

        private void Bot_StateChanged(TradeBotModel2 bot)
        {
            switch (bot.State)
            {
                case BotModelStates.Starting: ExecStatus = "Starting..."; break;
                case BotModelStates.Stopping: ExecStatus = "Stopping..."; break;
                case BotModelStates.Running: ExecStatus = "Running"; break;
                case BotModelStates.Stopped: ExecStatus = "Idle"; break;
            }
           
            NotifyOfPropertyChange(nameof(ExecStatus));
            NotifyOfPropertyChange(nameof(CanStartStop));
            NotifyOfPropertyChange(nameof(IsStarted));
        }
    }
}
