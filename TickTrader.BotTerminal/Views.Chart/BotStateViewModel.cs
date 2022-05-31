using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Server.PublicAPI.Converters;

namespace TickTrader.BotTerminal
{
    internal class BotStateViewModel : Screen
    {
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private AlgoEnvironment _algoEnv;
        private IAlgoAgent _agent;
        private string _botId;
        private bool _isInitialized;
        private bool _isActivated;

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
        public bool CanBrowse => !(Bot?.Model.IsRemote ?? true) || Bot.Agent.Model.AccessManager.CanGetBotFolderInfo(PluginFolderInfo.Types.PluginFolderId.BotLogs.ToApi());
        public string ExecStatus { get; private set; }
        public string BotInfo => string.Join(Environment.NewLine, GetBotInfo());
        public int ErrorsCount => Bot?.Model.Journal.MessageCount[JournalMessageType.Error] ?? 0;
        public BotJournalViewModel BotJournal { get; private set; }
        public bool IsRemote => Bot?.Model.IsRemote ?? true;

        //public override void TryClose(bool? dialogResult = default(bool?))
        //{
        //    base.TryClose(dialogResult);

        //    Deinit();
        //}

        public override Task TryCloseAsync(bool? dialogResult = null)
        {
            Deinit();
            return base.TryCloseAsync(dialogResult);
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

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);

            _isActivated = true;
            Init();
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            await base.OnDeactivateAsync(close, cancellationToken);

            _isActivated = false;
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
            if (!_isInitialized && _isActivated)
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
                case PluginModelInfo.Types.PluginState.Stopping: ExecStatus = "Stopping..."; break;
                case PluginModelInfo.Types.PluginState.Stopped: ExecStatus = "Stopped"; break;
                case PluginModelInfo.Types.PluginState.Running: ExecStatus = "Running"; break;
                case PluginModelInfo.Types.PluginState.Starting: ExecStatus = "Starting..."; break;
                case PluginModelInfo.Types.PluginState.Faulted: ExecStatus = "Faulted"; break;
                case PluginModelInfo.Types.PluginState.Broken: ExecStatus = "Broken"; break;
                case PluginModelInfo.Types.PluginState.Reconnecting: ExecStatus = "Reconnecting..."; break;
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
            res.Add($"AlgoServer: {Bot.Agent.Name}");
            if (Bot.Agent.Model.Accounts.Snapshot.TryGetValue(Bot.AccountId, out var acc))
                res.Add($"Account: {acc.DisplayName}");
            else res.Add($"Account Id: {Bot.AccountId}");
            res.Add($"Instance Id: {Bot.InstanceId}");
            res.Add("------------ Permissions ------------");
            Bot.Model.Config.Permissions.ToPermissionsList().ForEach(p => res.Add(p));
            if (Bot.Model.Descriptor != null)
            {
                res.Add("------------ Plugin Info ------------");
                res.Add($"Name: {Bot.Model.Descriptor.DisplayName}");
                res.Add($"Version: {Bot.Model.Descriptor.Version}");
            }
            if (Bot.Model.Config != null)
            {
                var config = Bot.Model.Config;
                res.Add("------------ Plugin Config ------------");
                res.Add($"Algo Package Id: {config.Key.PackageId}");
                res.Add($"Symbol: {config.MainSymbol.Name}");
                res.Add($"Timeframe: {config.Timeframe}");
                res.Add($"Model: {config.ModelTimeframe}");
                if (Bot.Model.Descriptor != null)
                    res.Add($"Show on chart: {Bot.Model.Descriptor.SetupMainSymbol}");
                var properties = config.UnpackProperties();
                if (properties.Any())
                {
                    res.Add("");
                    res.Add("------------ Parameters ------------");
                    res.AddRange(properties.Select(x => x as IParameterConfig).Where(x => x != null)
                        .Select(x => $"{x.PropertyId}: {x.ValObj}").OrderBy(x => x).ToArray());
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
