using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.IO;

namespace TickTrader.BotTerminal
{
    public interface IDockManagerServiceProvider
    {
        event Action<Stream> SaveLayoutEvent;

        event Action<Stream> LoadLayoutEvent;

        event Action<string> ToggleViewEvent;


        void SetViewVisibility(string contentId, bool opened);
    }


    public class DockManagerService : PropertyChangedBase, IDockManagerServiceProvider
    {
        public const string Tab_Symbols = "Tab_Symbols";
        public const string Tab_Bots = "Tab_Bots";
        public const string Tab_Algo = "Tab_Algo";
        public const string Tab_Trade = "Tab_Trade";
        public const string Tab_History = "Tab_History";
        public const string Tab_Journal = "Tab_Journal";
        public const string Tab_BotJournal = "Tab_BotJournal";


        private Dictionary<string, bool> _viewOpened;
        private event Action<Stream> _saveLayoutEvent;
        private event Action<Stream> _loadLayoutEvent;
        private event Action<string> _toggleViewEvent;


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

        public bool IsBotJournalOpened
        {
            get { return _viewOpened[Tab_BotJournal]; }
            set
            {
                if (_viewOpened[Tab_BotJournal] == value)
                    return;

                ToggleView(Tab_BotJournal);
            }
        }

        #endregion Bindable properties


        public DockManagerService()
        {
            _viewOpened = new Dictionary<string, bool>
            {
                {Tab_Symbols, false },
                {Tab_Bots, false },
                {Tab_Algo, false },
                {Tab_Trade, false },
                {Tab_History, false },
                {Tab_Journal, false },
                {Tab_BotJournal, false },
            };
        }


        public void SaveLayout(Stream stream)
        {
            _saveLayoutEvent?.Invoke(stream);
        }

        public void LoadLayout(Stream stream)
        {
            _loadLayoutEvent?.Invoke(stream);
        }

        public void ToggleView(string contentId)
        {
            _toggleViewEvent?.Invoke(contentId);
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


        void IDockManagerServiceProvider.SetViewVisibility(string contentId, bool opened)
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
                case Tab_BotJournal: NotifyOfPropertyChange(nameof(IsBotJournalOpened)); break;
            }
        }

        #endregion IDockManagerServiceProvider
    }
}
