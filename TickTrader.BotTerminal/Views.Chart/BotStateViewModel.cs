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

        public BotStateViewModel(AlgoBotViewModel bot, AlgoEnvironment algoEnv)
        {
            _algoEnv = algoEnv;
            Bot = bot;
            Bot.Model.StateChanged += Bot_StateChanged;
            Bot.Model.ConfigurationChanged += BotConfigurationChanged;
            DisplayName = "Status: " + bot.InstanceId;
            BotName = Bot.InstanceId;
            Bot_StateChanged(Bot.Model);

            Bot.Model.Journal.Records.CollectionChanged += (sender, e) => NotifyOfPropertyChange(nameof(ErrorsCount));
            BotJournal = new BotJournalViewModel(Bot.Model);

            Bot.Model.SubscribeToStatus();
            Bot.Model.SubscribeToLogs();
        }

        private void BotConfigurationChanged(ITradeBot obj)
        {
            NotifyOfPropertyChange(nameof(BotInfo));
        }

        public AlgoBotViewModel Bot { get; private set; }
        public bool IsRunning => Bot.IsRunning;
        public bool CanStartStop => Bot.CanStartStop;
        public bool CanBrowse => !Bot.Model.IsRemote;
        public string BotName { get; private set; }
        public string ExecStatus { get; private set; }
        public string BotInfo => string.Join(Environment.NewLine, GetBotInfo());
        public int ErrorsCount => Bot.Model.Journal.MessageCount[JournalMessageType.Error];
        public BotJournalViewModel BotJournal { get; }

        public override void TryClose(bool? dialogResult = default(bool?))
        {
            base.TryClose(dialogResult);

            Bot.Model.ConfigurationChanged -= BotConfigurationChanged;
            //Bot.Removed -= Bot_Removed;
            Bot.Model.StateChanged -= Bot_StateChanged;
        }

        public void StartStop()
        {
            Bot.StartStop();
        }

        public void OpenSettings()
        {
            Bot.OpenSettings();
        }

        public void Clear()
        {
            BotJournal.Clear();
        }

        public void Browse()
        {
            BotJournal.Browse();
        }

        private void Bot_Removed(TradeBotModel bot)
        {
            TryClose();
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
            NotifyOfPropertyChange(nameof(IsRunning));
            NotifyOfPropertyChange(nameof(CanStartStop));
        }

        private IEnumerable<string> GetBotInfo()
        {
            var res = new List<string>();
            res.Add($"Instance Id: {Bot.InstanceId}");
            res.Add("------------ Permissions ------------");
            res.Add(Bot.Model.Config.Permissions.ToString());
            if (Bot.Model.Descriptor != null)
            {
                res.Add("------------ Plugin Info ------------");
                res.Add($"Name: {Bot.Model.Descriptor.DisplayName}");
                res.Add($"Version: {Bot.Model.Descriptor.Version}");
                //res.Add($"Package Name: {Bot.PackageRef.Name}");
                //res.Add($"Package Location: {Bot.PackageRef.Location}");
            }
            if (Bot.Model.Config?.Properties.Any() ?? false)
            {
                res.Add("");
                res.Add("------------ Parameters ------------");
                res.AddRange(Bot.Model.Config.Properties.Select(x => x as Algo.Common.Model.Config.Parameter).Where(x => x != null)
                    .Select(x => $"{x.Id}: {x.ValObj}").OrderBy(x => x).ToArray());
            }
            return res;
        }
    }
}
