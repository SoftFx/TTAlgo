using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class BotStateViewModel : Screen, IWindowModel
    {
        private ToolWindowsManager _wndManager;

        public BotStateViewModel(TradeBotModel bot, ToolWindowsManager wndManager)
        {
            _wndManager = wndManager;
            this.Bot = bot;
            Bot.Removed += Bot_Removed;
            Bot.StateChanged += Bot_StateChanged;
            Bot.ConfigurationChanged += BotConfigurationChanged;
            Bot.CustomStatusChanged += Bot_CustomStatusChanged;
            Bot.StateViewOpened = true;
            DisplayName = "Status: " + bot.InstanceId;
            BotName = Bot.InstanceId;
            Bot_StateChanged(Bot);
            Bot_CustomStatusChanged(Bot);
        }

        private void BotConfigurationChanged(TradeBotModel obj)
        {
            NotifyOfPropertyChange(nameof(BotInfo));
        }

        public TradeBotModel Bot { get; private set; }
        public string BotName { get; private set; }
        public string ExecStatus { get; private set; }
        public string CustomStatus { get; private set; }
        public bool IsStarted { get { return Bot.State == BotModelStates.Running || Bot.State == BotModelStates.Stopping; } }
        public bool CanStartStop { get { return Bot.State == BotModelStates.Running || Bot.State == BotModelStates.Stopped; } }
        public bool CanOpenSettings { get { return Bot.State == BotModelStates.Stopped; } }
        public string BotInfo => string.Join(Environment.NewLine, GetBotInfo());
        public bool HasParams => Bot.Setup.Parameters.Any();

        public override void TryClose(bool? dialogResult = default(bool?))
        {
            base.TryClose(dialogResult);

            Bot.ConfigurationChanged -= BotConfigurationChanged;
            Bot.Removed -= Bot_Removed;
            Bot.StateChanged -= Bot_StateChanged;
            Bot.CustomStatusChanged -= Bot_CustomStatusChanged;
            Bot.StateViewOpened = false;
        }

        public async void StartStop()
        {
            if (Bot.State == BotModelStates.Running)
                await Bot.Stop();
            else if (Bot.State == BotModelStates.Stopped)
                Bot.Start();
            else if (Bot.State == BotModelStates.Stopping)
                throw new Exception("StartStop() cannot be called when Bot is stopping!");
        }

        public void OpenSettings()
        {
            var key = $"BotSettings {Bot.InstanceId}";

            var wnd = _wndManager.GetWindow(key);
            if (wnd != null)
            {
                wnd.Activate();
            }
            else
            {
                var pSetup = new PluginSetupViewModel(Bot);
                pSetup.Closed += PluginSetupViewClosed;

                _wndManager.OpenWindow(key, pSetup);
            }
        }

        private void PluginSetupViewClosed(PluginSetupViewModel setupVM, bool dialogResult)
        {
            if (dialogResult)
            {
                Bot.Configurate(setupVM.Setup, setupVM.Permissions, setupVM.Isolated);
            }
        }

        private void Bot_Removed(TradeBotModel bot)
        {
            TryClose();
        }

        private void Bot_CustomStatusChanged(TradeBotModel bot)
        {
            CustomStatus = bot.CustomStatus;
            NotifyOfPropertyChange(nameof(CustomStatus));
        }

        private void Bot_StateChanged(TradeBotModel bot)
        {
            switch (bot.State)
            {
                case BotModelStates.Stopping: ExecStatus = "Stopping..."; break;
                case BotModelStates.Running: ExecStatus = "Running"; break;
                case BotModelStates.Stopped: ExecStatus = "Idle"; break;
            }

            NotifyOfPropertyChange(nameof(ExecStatus));
            NotifyOfPropertyChange(nameof(CanOpenSettings));
            NotifyOfPropertyChange(nameof(CanStartStop));
            NotifyOfPropertyChange(nameof(IsStarted));
            NotifyOfPropertyChange(nameof(BotInfo));
            NotifyOfPropertyChange(nameof(HasParams));
        }

        private IEnumerable<string> GetBotInfo()
        {
            var res = new List<string>();
            res.Add($"Instance Id: {Bot.InstanceId}");
            res.Add($"Isolation: {(Bot.Isolated ? "Enabled" : "Disabled")}");
            res.Add("");
            res.Add("------------ Permissions ------------");
            res.Add(Bot.Permissions.ToString());
            res.Add("------------ Plugin Info ------------");
            res.Add($"Name: {Bot.Setup.Descriptor.UserDisplayName}");
            res.Add($"Version: {Bot.Setup.Descriptor.Version}");
            res.Add($"File Path: {Bot.PluginFilePath}");
            if (Bot.Setup.HasParams)
            {
                res.Add("");
                res.Add("------------ Parameters ------------");
                res.AddRange(Bot.Setup.Parameters.Select(x => x.ToString()).OrderBy(x => x).ToArray());
            }
            return res;
        }
    }
}
