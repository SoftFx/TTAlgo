using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TickTrader.Algo.Common.Info;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;

namespace TickTrader.BotTerminal
{
    internal class BotStateViewModel : Screen
    {
        private AlgoEnvironment _algoEnv;
        private BotMessageFilter _botLogsFilter = new BotMessageFilter();
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public BotStateViewModel(TradeBotModel bot, AlgoEnvironment algoEnv)
        {
            _algoEnv = algoEnv;
            Bot = bot;
            Bot.StateChanged += Bot_StateChanged;
            Bot.ConfigurationChanged += BotConfigurationChanged;
            Bot.StatusChanged += Bot_CustomStatusChanged;
            Bot.StateViewOpened = true;
            DisplayName = "Status: " + bot.InstanceId;
            BotName = Bot.InstanceId;
            Bot_StateChanged(Bot);
            Bot_CustomStatusChanged(Bot);

            Bot.Journal.Records.CollectionChanged += (sender, e) => NotifyOfPropertyChange(nameof(ErrorsCount));
            BotJournal = new BotJournalViewModel(Bot);
        }

        private void BotConfigurationChanged(ITradeBot obj)
        {
            NotifyOfPropertyChange(nameof(BotInfo));
        }

        public TradeBotModel Bot { get; private set; }
        public string BotName { get; private set; }
        public string ExecStatus { get; private set; }
        public string CustomStatus { get; private set; }
        public bool IsStarted { get { return Bot.State == PluginStates.Running || Bot.State == PluginStates.Stopping; } }
        public bool CanStartStop { get { return Bot.State == PluginStates.Running || Bot.State == PluginStates.Stopped; } }
        public bool CanOpenSettings { get { return Bot.State == PluginStates.Stopped; } }
        public string BotInfo => string.Join(Environment.NewLine, GetBotInfo());
        public bool HasParams => Bot.Setup.Parameters.Any();
        public int ErrorsCount => Bot.Journal.MessageCount[JournalMessageType.Error];
        public BotJournalViewModel BotJournal { get; }

        public override void TryClose(bool? dialogResult = default(bool?))
        {
            base.TryClose(dialogResult);

            Bot.ConfigurationChanged -= BotConfigurationChanged;
            //Bot.Removed -= Bot_Removed;
            Bot.StateChanged -= Bot_StateChanged;
            Bot.StatusChanged -= Bot_CustomStatusChanged;
            Bot.StateViewOpened = false;
        }

        public async void StartStop()
        {
            if (Bot.State == PluginStates.Running)
                await Bot.Stop();
            else if (Bot.State == PluginStates.Stopped)
                Bot.Start();
            else if (Bot.State == PluginStates.Stopping)
                throw new Exception("StartStop() cannot be called when Bot is stopping!");
        }

        public void OpenSettings()
        {
            _algoEnv.LocalAgentVM.OpenBotSetup(Bot.ToInfo());
        }

        private void Bot_Removed(TradeBotModel bot)
        {
            TryClose();
        }

        private void Bot_CustomStatusChanged(ITradeBot bot)
        {
            CustomStatus = bot.Status;
            NotifyOfPropertyChange(nameof(CustomStatus));
        }

        private void Bot_StateChanged(ITradeBot bot)
        {
            switch (bot.State)
            {
                case PluginStates.Stopping: ExecStatus = "Stopping..."; break;
                case PluginStates.Stopped: ExecStatus = "Stopped"; break;
                case PluginStates.Running: ExecStatus = "Running"; break;
                case PluginStates.Starting: ExecStatus = "Starting..."; break;
                case PluginStates.Faulted: ExecStatus = "Faulted"; break;
                case PluginStates.Broken: ExecStatus = "Broken"; break;
                case PluginStates.Reconnecting: ExecStatus = "Reconnecting..."; break;
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
            res.Add("------------ Permissions ------------");
            res.Add(Bot.Config.Permissions.ToString());
            if (Bot.PluginRef != null)
            {
                res.Add("------------ Plugin Info ------------");
                res.Add($"Name: {Bot.PluginRef.Metadata.Descriptor.DisplayName}");
                res.Add($"Version: {Bot.PluginRef.Metadata.Descriptor.Version}");
                res.Add($"Package Name: {Bot.PackageRef.Name}");
                res.Add($"Package Location: {Bot.PackageRef.Location}");
            }
            if (Bot.Setup?.Parameters.Any() ?? false)
            {
                res.Add("");
                res.Add("------------ Parameters ------------");
                res.AddRange(Bot.Setup.Parameters.Select(x => x.ToString()).OrderBy(x => x).ToArray());
            }
            return res;
        }

        public void Clear()
        {
            BotJournal.Clear();
        }

        public void Browse()
        {
            BotJournal.Browse();
        }
    }
}
