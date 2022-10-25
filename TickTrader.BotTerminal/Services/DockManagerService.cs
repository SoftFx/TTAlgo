using Caliburn.Micro;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TickTrader.BotTerminal
{
    internal interface IDockManagerServiceProvider
    {
        event Action<Stream> SaveLayoutEvent;

        event Action<Stream> LoadLayoutEvent;

        event Action<string> ToggleViewEvent;

        event Action<string> ShowViewEvent;

        event Action<string> RemoveViewEvent;

        event Action<IScreen, string> RegisterViewToLayout;

        event System.Action RemoveViewsEvent;

        System.Action Initialized { get; }


        void ViewVisibilityChanged(string contentId, bool opened);

        IScreen GetScreen(string contentId);

        bool ShouldClose(string contentId);
    }


    internal class DockManagerService : PropertyChangedBase, IDockManagerServiceProvider
    {
        private static readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();


        public const string Tab_Symbols = "Tab_Symbols";
        public const string Tab_Bots = "Tab_Bots";
        public const string Tab_Algo = "Tab_Algo";
        public const string Tab_Trade = "Tab_Trade";
        public const string Tab_History = "Tab_History";
        public const string Tab_Journal = "Tab_Journal";
        public const string Tab_Alert = "Tab_Alert";


        private AlgoEnvironment _algoEnv;
        private Dictionary<string, bool> _viewOpened;
        private event Action<Stream> _saveLayoutEvent;
        private event Action<Stream> _loadLayoutEvent;
        private event Action<string> _toggleViewEvent;
        private event Action<string> _showViewEvent;
        private event Action<string> _removeViewEvent;
        private event System.Action _removeViewsEvent;
        private event Action<IScreen, string> _registerViewToLayout;


        #region Bindable properties

        public bool IsSymbolsOpened
        {
            get { return _viewOpened[Tab_Symbols]; }
            set
            {
                if (_viewOpened[Tab_Symbols] == value)
                    return;

                ToggleView(Tab_Symbols);
            }
        }

        public bool IsBotsOpened
        {
            get { return _viewOpened[Tab_Bots]; }
            set
            {
                if (_viewOpened[Tab_Bots] == value)
                    return;

                ToggleView(Tab_Bots);
            }
        }

        public bool IsAlgoOpened
        {
            get { return _viewOpened[Tab_Algo]; }
            set
            {
                if (_viewOpened[Tab_Algo] == value)
                    return;

                ToggleView(Tab_Algo);
            }
        }

        public bool IsTradeOpened
        {
            get { return _viewOpened[Tab_Trade]; }
            set
            {
                if (_viewOpened[Tab_Trade] == value)
                    return;

                ToggleView(Tab_Trade);
            }
        }

        public bool IsHistoryOpened
        {
            get { return _viewOpened[Tab_History]; }
            set
            {
                if (_viewOpened[Tab_History] == value)
                    return;

                ToggleView(Tab_History);
            }
        }

        public bool IsJournalOpened
        {
            get { return _viewOpened[Tab_Journal]; }
            set
            {
                if (_viewOpened[Tab_Journal] == value)
                    return;

                ToggleView(Tab_Journal);
            }
        }

        public bool IsAlertOpened
        {
            get { return _viewOpened[Tab_Alert]; }
            set
            {
                if (_viewOpened[Tab_Alert] == value)
                    return;

                ToggleView(Tab_Alert);
            }
        }

        public System.Action Initialized { get; }

        #endregion Bindable properties


        public DockManagerService(AlgoEnvironment algoEnv, System.Action initialized)
        {
            _algoEnv = algoEnv;

            _viewOpened = new Dictionary<string, bool>
            {
                {Tab_Symbols, false },
                {Tab_Bots, false },
                {Tab_Algo, false },
                {Tab_Trade, false },
                {Tab_History, false },
                {Tab_Journal, false },
                {Tab_Alert, false },
            };

            Initialized = initialized;
        }


        public void SaveLayoutSnapshot(ProfileStorageModel profileStorage)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    _saveLayoutEvent?.Invoke(stream);
                    profileStorage.Layout = Encoding.UTF8.GetString(stream.ToArray());
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to save layout snapshot");
            }
        }

        public void LoadLayoutSnapshot(ProfileStorageModel profileStorage)
        {
            try
            {
                if (profileStorage.Layout != null)
                {
                    using (var stream = new MemoryStream())
                    {
                        var bytes = Encoding.UTF8.GetBytes(profileStorage.Layout);
                        stream.Write(bytes, 0, bytes.Length);
                        stream.Seek(0, SeekOrigin.Begin);
                        _loadLayoutEvent?.Invoke(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load layout snapshot");
            }
        }


        private void ToggleView(string contentId)
        {
            _toggleViewEvent?.Invoke(contentId);
        }

        public void ShowView(string contentId)
        {
            _showViewEvent?.Invoke(contentId);
        }

        public void RemoveView(string contentId)
        {
            _removeViewEvent?.Invoke(contentId);
        }

        public void RemoveViews()
        {
            _removeViewsEvent?.Invoke();
        }

        public void RegisterView(IScreen screen, string key)
        {
            _registerViewToLayout?.Invoke(screen, key);
        }

        #region IDockManagerServiceProvider

        event Action<Stream> IDockManagerServiceProvider.SaveLayoutEvent
        {
            add { _saveLayoutEvent += value; }
            remove { _saveLayoutEvent -= value; }
        }

        event Action<Stream> IDockManagerServiceProvider.LoadLayoutEvent
        {
            add { _loadLayoutEvent += value; }
            remove { _loadLayoutEvent -= value; }
        }

        event Action<string> IDockManagerServiceProvider.ToggleViewEvent
        {
            add { _toggleViewEvent += value; }
            remove { _toggleViewEvent -= value; }
        }

        event Action<string> IDockManagerServiceProvider.ShowViewEvent
        {
            add { _showViewEvent += value; }
            remove { _showViewEvent -= value; }
        }

        event Action<IScreen, string> IDockManagerServiceProvider.RegisterViewToLayout
        {
            add { _registerViewToLayout += value; }
            remove { _registerViewToLayout -= value; }
        }

        event Action<string> IDockManagerServiceProvider.RemoveViewEvent
        {
            add { _removeViewEvent += value; }
            remove { _removeViewEvent -= value; }
        }

        event System.Action IDockManagerServiceProvider.RemoveViewsEvent
        {
            add { _removeViewsEvent += value; }
            remove { _removeViewsEvent -= value; }
        }


        void IDockManagerServiceProvider.ViewVisibilityChanged(string contentId, bool opened)
        {
            if (!_viewOpened.ContainsKey(contentId))
                return;
            if (_viewOpened[contentId] == opened)
                return;

            _viewOpened[contentId] = opened;
            switch (contentId)
            {
                case Tab_Symbols: NotifyOfPropertyChange(nameof(IsSymbolsOpened)); break;
                case Tab_Bots: NotifyOfPropertyChange(nameof(IsBotsOpened)); break;
                case Tab_Algo: NotifyOfPropertyChange(nameof(IsAlgoOpened)); break;
                case Tab_Trade: NotifyOfPropertyChange(nameof(IsTradeOpened)); break;
                case Tab_History: NotifyOfPropertyChange(nameof(IsHistoryOpened)); break;
                case Tab_Journal: NotifyOfPropertyChange(nameof(IsJournalOpened)); break;
                case Tab_Alert: NotifyOfPropertyChange(nameof(IsAlertOpened)); break;
            }
        }

        IScreen IDockManagerServiceProvider.GetScreen(string contentId)
        {
            if (ContentIdProvider.TryParse(contentId, out var agent, out var botId))
            {
                var agentModel = _algoEnv.Agents.Snapshot.FirstOrDefault(a => a.Name == agent)?.Model;
                if (agentModel != null)
                    return new BotStateViewModel(_algoEnv, agentModel, botId);
            }
            return null;
        }

        bool IDockManagerServiceProvider.ShouldClose(string contentId)
        {
            return ContentIdProvider.TryParse(contentId, out var agent, out var bot);
        }

        #endregion IDockManagerServiceProvider
    }


    internal static class ContentIdProvider
    {
        public static string Generate(string agentName, string botId)
        {
            return $"bot/{agentName}/{botId}";
        }

        public static bool TryParse(string contentId, out string agentName, out string botId)
        {
            agentName = null;
            botId = null;

            if (contentId.StartsWith("bot/"))
            {
                var parts = contentId.Split('/');
                if (parts.Length == 3)
                {
                    agentName = parts[1];
                    botId = parts[2];
                    return true;
                }
            }

            return false;
        }
    }
}
