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
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;

namespace TickTrader.BotTerminal
{
    internal class BotStateViewModel : Screen
    {
        private IShell _shell;
        private BotMessageFilter _botLogsFilter = new BotMessageFilter();
        private ObservableCollection<BotNameFilterEntry> _botNameFilterEntries = new ObservableCollection<BotNameFilterEntry>();
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public BotStateViewModel(TradeBotModel bot, IShell shell)
        {
            _shell = shell;
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

            _botNameFilterEntries.Add(new BotNameFilterEntry("All", BotNameFilterType.All));
            if (!Bot.Host.Journal.Records.ContainsKey(Bot.InstanceId))
                Bot.Host.Journal.Records.Add(Bot.InstanceId, new Journal<BotMessage>(1000));
            BotLogs = CollectionViewSource.GetDefaultView(Bot.Host.Journal.Records[Bot.InstanceId].Records);
            Bot.Host.Journal.Records[Bot.InstanceId].Records.CollectionChanged += (sender, e) => NotifyOfPropertyChange(nameof(ErrorsCount));
            Bot.Host.Journal.Statistics.Items.Updated += args =>
            {
                if (args.Action == DLinqAction.Insert)
                    _botNameFilterEntries.Add(new BotNameFilterEntry(args.Key, BotNameFilterType.SpecifiedName));
                else if (args.Action == DLinqAction.Remove)
                {
                    var entry = _botNameFilterEntries.FirstOrDefault((e) => e.Type == BotNameFilterType.SpecifiedName && e.Name == args.Key);

                    if (selectedBotNameFilter == entry)
                        SelectedBotNameFilter = _botNameFilterEntries.First();

                    if (entry != null)
                        _botNameFilterEntries.Remove(entry);
                }
            };
            SelectedBotNameFilter = _botNameFilterEntries.First();
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
        public ICollectionView BotLogs { get; private set; }
        public int ErrorsCount => Bot.Host.Journal.Records[Bot.InstanceId].Records.Where(v => v.Type == JournalMessageType.Error).Count();

        public MessageTypeFilter TypeFilter
        {
            get { return _botLogsFilter.MessageTypeCondition; }
            set
            {
                if (_botLogsFilter.MessageTypeCondition != value)
                {
                    _botLogsFilter.MessageTypeCondition = value;
                    NotifyOfPropertyChange(nameof(TypeFilter));
                    ApplyFilter();
                }
            }
        }
        public string TextFilter
        {
            get { return _botLogsFilter.TextFilter; }
            set
            {
                if (_botLogsFilter.TextFilter != value)
                {
                    _botLogsFilter.TextFilter = value;
                    NotifyOfPropertyChange(nameof(TextFilter));
                    ApplyFilter();
                }
            }
        }
        private BotNameFilterEntry selectedBotNameFilter;
        public BotNameFilterEntry SelectedBotNameFilter
        {
            get { return selectedBotNameFilter; }
            set
            {
                _botLogsFilter.BotCondition = value;

                selectedBotNameFilter = value;
                NotifyOfPropertyChange(nameof(SelectedBotNameFilter));
                ApplyFilter();
            }
        }

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

            _shell.ToolWndManager.OpenOrActivateWindow(key, () =>
            {
                var pSetup = new SetupPluginViewModel(_shell.Agent, Bot.ToInfo());
                pSetup.Closed += SetupPluginViewClosed;
                return pSetup;
            });
        }

        private void SetupPluginViewClosed(SetupPluginViewModel setupVM, bool dialogResult)
        {
            if (dialogResult)
            {
                Bot.Configurate(setupVM.GetConfig());
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

        private void ApplyFilter()
        {
            if (BotLogs != null)
                BotLogs.Filter = msg => _botLogsFilter.Filter((BotMessage)msg);
        }

        public void Clear()
        {
            Bot.Host.Journal.Records[Bot.InstanceId].Clear();
        }

        public void Browse()
        {
            try
            {
                var logDir = Path.Combine(EnvService.Instance.BotLogFolder, Bot.InstanceId);
                Directory.CreateDirectory(logDir);
                Process.Start(logDir);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to browse bot journal folder");
            }
        }
    }
}
