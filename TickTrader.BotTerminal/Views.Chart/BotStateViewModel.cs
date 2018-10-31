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
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public BotStateViewModel(AlgoBotViewModel bot)
        {
            Bot = bot;
            DisplayName = $"Status: {bot.InstanceId} ({bot.Agent.Name})";
            BotName = Bot.InstanceId;

            Bot.Model.Journal.Records.CollectionChanged += (sender, e) => NotifyOfPropertyChange(nameof(ErrorsCount));
            BotJournal = new BotJournalViewModel(Bot);
        }

        public AlgoBotViewModel Bot { get; private set; }
        public bool IsRunning => Bot.IsRunning;
        public bool CanStartStop => Bot.CanStartStop;
        public bool CanBrowse => !Bot.Model.IsRemote || Bot.Agent.Model.AccessManager.CanGetBotFolderInfo(BotFolderId.BotLogs);
        public string BotName { get; private set; }
        public string ExecStatus { get; private set; }
        public string BotInfo => string.Join(Environment.NewLine, GetBotInfo());
        public int ErrorsCount => Bot.Model.Journal.MessageCount[JournalMessageType.Error];
        public BotJournalViewModel BotJournal { get; }
        public bool IsRemote => Bot.Model.IsRemote;

        public override void TryClose(bool? dialogResult = default(bool?))
        {
            base.TryClose(dialogResult);

            Bot.Model.Updated -= Bot_Updated;
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

        protected override void OnActivate()
        {
            base.OnActivate();

            Bot.Model.StateChanged += Bot_StateChanged;
            Bot.Model.Updated += Bot_Updated;

            Bot.Model.SubscribeToStatus();
            Bot.Model.SubscribeToLogs();

            Bot_StateChanged(Bot.Model);
            Bot_Updated(Bot.Model);
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);

            Bot.Model.Updated -= Bot_Updated;
            Bot.Model.StateChanged -= Bot_StateChanged;

            Bot.Model.UnsubscribeFromStatus();
            Bot.Model.UnsubscribeFromLogs();
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

        private void Bot_Updated(ITradeBot obj)
        {
            NotifyOfPropertyChange(nameof(BotInfo));
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
            }
            if (Bot.Model.Config != null)
            {
                res.Add($"Package Name: {Bot.Model.Config.Key.PackageName}");
                res.Add($"Package Location: {Bot.Model.Config.Key.PackageLocation}");
                res.Add($"Symbol: {Bot.Model.Config.MainSymbol.Name}");
                res.Add($"Timeframe: {Bot.Model.Config.TimeFrame}");
                res.Add($"Show on chart: {Bot.Model.Descriptor.SetupMainSymbol}");
                if (Bot.Model.Config.Properties.Any())
                {
                    res.Add("");
                    res.Add("------------ Parameters ------------");
                    res.AddRange(Bot.Model.Config.Properties.Select(x => x as Algo.Common.Model.Config.Parameter).Where(x => x != null)
                        .Select(x => $"{x.Id}: {x.ValObj}").OrderBy(x => x).ToArray());
                }
            }
            return res;
        }
    }
}
