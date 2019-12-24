using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class SearchItemViewModel<T> : PropertyChangedBase where T : BaseJournalMessage
    {
        private readonly Predicate<T> _filter;
        private readonly System.Action _refresh;

        private ObservableCircularList<T> _list;
        private int _countMatches = 0;
        private string _filterString;

        private T _selectItem;

        public SearchItemViewModel(ObservableCircularList<T> list, Predicate<T> filter = null, System.Action refresh = null)
        {
            _list = list;
            _filter = filter;
            _refresh = refresh;
        }

        public bool HasResult => CountMatches > 0;

        public string MatchesResult => $"Search result: {CountMatches}";

        public T SelectItem
        {
            get => _selectItem;
            set
            {
                _selectItem = value;
                NotifyOfPropertyChange(nameof(SelectItem));
            }
        }

        public int CountMatches
        {
            get => _countMatches;
            set
            {
                if (_countMatches == value)
                    return;

                _countMatches = value;
                NotifyOfPropertyChange(nameof(CountMatches));
                NotifyOfPropertyChange(nameof(HasResult));
                NotifyOfPropertyChange(nameof(MatchesResult));
            }
        }

        public string FilterString
        {
            get => _filterString;
            set
            {
                _filterString = value;
                NotifyOfPropertyChange(nameof(FilterString));
                _refresh?.Invoke();

                FullCalculateMatches();
            }
        }

        public void FindNextItem() => FindNextSubstring();

        public void FindPreviuosItem() => FindPreviosSubstring();

        public void CalculateMatches(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(FilterString) && e.Action != NotifyCollectionChangedAction.Reset)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (var item in e.NewItems)
                        {
                            if (!(item is EventMessage mes))
                                continue;

                            if (mes.Message.IndexOf(FilterString) >= 0)
                                CountMatches++;
                        }

                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (var item in e.OldItems)
                        {
                            if (!(item is EventMessage mes))
                                continue;

                            if (mes.Message.IndexOf(FilterString) >= 0)
                                CountMatches--;
                        }

                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        CountMatches = 0;
                        break;
                    }
                default:
                    return;
            }
        }

        public void FullCalculateMatches()
        {
            CountMatches = 0;

            if (string.IsNullOrEmpty(FilterString))
                return;

            SelectItem = null;
            //FindPreviosSubstring(true); //Sometimes XAML selects several lines, although SelectionMode="Single"

            foreach (var item in _list)
                if (item.Message.IndexOf(FilterString, StringComparison.CurrentCultureIgnoreCase) > -1)
                    CountMatches++;
        }

        internal void FindNextSubstring(bool include = false)
        {
            if (string.IsNullOrEmpty(FilterString) || _list == null || _list.Count == 0)
                return;

            int to = _list.IndexOf(SelectItem ?? _list.First());

            if (to == -1)
                return;

            for (int i = to + (include ? 0 : 1); i < _list.Count; ++i)
            {
                var record = _list[i];
                var index = record.Message.IndexOf(FilterString, StringComparison.CurrentCultureIgnoreCase);

                if (index != -1)
                {
                    if (_filter != null && !_filter(record))
                        continue;

                    SelectItem = record;
                    return;
                }
            }
        }

        internal void FindPreviosSubstring(bool include = false)
        {
            if (string.IsNullOrEmpty(FilterString) || _list == null || _list.Count == 0)
                return;

            int from = _list.IndexOf(SelectItem ?? _list.Last());

            if (from == -1)
                return;

            for (int i = from - (include ? 0 : 1); i > -1; --i)
            {
                var record = _list[i];
                var index = record.Message.IndexOf(FilterString, StringComparison.CurrentCultureIgnoreCase);

                if (index != -1)
                {
                    if (_filter != null && !_filter(record))
                        continue;

                    SelectItem = record;
                    return;
                }
            }
        }
    }
}
