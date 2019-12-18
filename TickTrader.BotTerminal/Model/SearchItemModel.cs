using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class SearchItemModel<T> where T : BaseJournalMessage
    {
        private readonly Predicate<T> _filter;

        private ObservableCircularList<T> _list;

        public SearchItemModel(ObservableCircularList<T> list, Predicate<T> filter = null)
        {
            _list = list;
            _filter = filter;
        }

        internal BaseJournalMessage FindNextSubstring(BaseJournalMessage start, string serachString, bool include = false)
        {
            if (string.IsNullOrEmpty(serachString) || _list == null || _list.Count == 0)
                return start;

            int to = _list.IndexOf((T)start ?? _list.First());

            if (to == -1)
                return start;

            for (int i = to + (include ? 0 : 1); i < _list.Count; ++i)
            {
                var record = _list[i];
                var index = record.Message.IndexOf(serachString, StringComparison.CurrentCultureIgnoreCase);

                if (index != -1)
                {
                    if (_filter != null && !_filter(record))
                        continue;

                    return record;
                }             
            }

            return start;
        }

        internal BaseJournalMessage FindPreviosSubstring(BaseJournalMessage start, string searchString, bool include = false)
        {
            if (string.IsNullOrEmpty(searchString) || _list == null || _list.Count == 0)
                return start;

            int from = _list.IndexOf((T)start ?? _list.Last());

            if (from == -1)
                return start;

            for (int i = from - (include ? 0 : 1); i > -1; --i)
            {
                var record = _list[i];
                var index = record.Message.IndexOf(searchString, StringComparison.CurrentCultureIgnoreCase);

                if (index != -1)
                {
                    if (_filter != null && !_filter(record))
                        continue;

                    return record;
                }
            }

            return start;
        }
    }
}
