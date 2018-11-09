using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

        private AlgoEnvironment _algoEnv;
        private IAlgoAgent _agent;
        private string _botId;
        private bool _isInitialized;

        public BotStateViewModel(AlgoEnvironment algoEnv, IAlgoAgent agent, string botId)
        {
            _algoEnv = algoEnv;
            _agent = agent;
            _botId = botId;
            DisplayName = $"Status: {botId} ({agent.Name})";
            _isInitialized = false;

            FindBot();

            _agent.Bots.Updated += Bots_Updated;
            _agent.AccessLevelChanged += OnAccessLevelChanged;
        }

        public AlgoBotViewModel Bot { get; private set; }
        public bool IsRunning => Bot?.IsRunning ?? false;
        public bool CanStartStop => Bot?.CanStartStop ?? false;
        public bool CanBrowse => !(Bot?.Model.IsRemote ?? true) || Bot.Agent.Model.AccessManager.CanGetBotFolderInfo(BotFolderId.BotLogs);
        public string ExecStatus { get; private set; }
        public string BotInfo => string.Join(Environment.NewLine, GetBotInfo());
        public int ErrorsCount => Bot?.Model.Journal.MessageCount[JournalMessageType.Error] ?? 0;
        public BotJournalViewModel BotJournal { get; private set; }
        public bool IsRemote => Bot?.Model.IsRemote ?? true;

        public override void TryClose(bool? dialogResult = default(bool?))
        {
            base.TryClose(dialogResult);

            Deinit();
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
            Bot.Browse();
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            Init();
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);

            Deinit();
        }

        private bool FindBot()
        {
            var bot = _algoEnv.Agents.Snapshot.FirstOrDefault(a => a.Name == _agent.Name)
                        ?.Bots.Snapshot.FirstOrDefault(b => b.InstanceId == _botId);
            if (bot == null)
                return false;

            Bot = bot;
            NotifyOfPropertyChange(nameof(Bot));
            NotifyOfPropertyChange(nameof(BotInfo));
            NotifyOfPropertyChange(nameof(CanBrowse));
            NotifyOfPropertyChange(nameof(IsRemote));
            return true;
        }

        private void Init()
        {
            if (!_isInitialized)
            {
                try
                {
                    if (!FindBot())
                        return;

                    Bot.Model.Journal.Records.CollectionChanged += BotJournal_CollectionChanged;
                    BotJournal = new BotJournalViewModel(Bot);
                    NotifyOfPropertyChange(nameof(BotJournal));

                    Bot.Model.StateChanged += Bot_StateChanged;
                    Bot.Model.Updated += Bot_Updated;

                    Bot.Model.SubscribeToStatus();
                    Bot.Model.SubscribeToLogs();

                    Bot_StateChanged(Bot.Model);
                    Bot_Updated(Bot.Model);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to init bot state");
                }
                _isInitialized = true;
            }
        }

        private void Deinit()
        {
            if (_isInitialized)
            {
                try
                {
                    Bot.Model.Journal.Records.CollectionChanged -= BotJournal_CollectionChanged;

                    Bot.Model.Updated -= Bot_Updated;
                    Bot.Model.StateChanged -= Bot_StateChanged;

                    Bot.Model.UnsubscribeFromStatus();
                    Bot.Model.UnsubscribeFromLogs();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to deinit bot state");
                }
                _isInitialized = false;
            }
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
            if (Bot == null)
                return Enumerable.Empty<string>();

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

        private void OnAccessLevelChanged()
        {
            NotifyOfPropertyChange(nameof(CanStartStop));
            NotifyOfPropertyChange(nameof(CanBrowse));
        }

        private void Bots_Updated(DictionaryUpdateArgs<string, ITradeBot> args)
        {
            switch (args.Action)
            {
                case DLinqAction.Insert:
                    if (_botId == args.NewItem.InstanceId)
                        Init();
                    break;
                case DLinqAction.Remove:
                    if (_botId == args.OldItem.InstanceId)
                        Deinit();
                    break;
            }

        }

        private void BotJournal_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            NotifyOfPropertyChange(nameof(ErrorsCount));
        }
    }
}
