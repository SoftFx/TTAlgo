using System.Collections.Generic;

namespace TickTrader.BotAgent.Configurator
{
    public class RefreshCounter : BaseViewModel
    {
        private SortedSet<string> _updatedFields;

        public delegate void ConfigurationStateChanged();

        public event ConfigurationStateChanged NewValuesEvent;
        public event ConfigurationStateChanged SaveValuesEvent;

        public bool Update => _updatedFields.Count > 0;

        public RefreshCounter()
        {
            _updatedFields = new SortedSet<string>();
        }

        public void CheckUpdate(string newValue, string oldValue, string field)
        {
            if (newValue != oldValue)
                AddUpdate(field);
            else
                DeleteUpdate(field);
        }

        public void AddUpdate(string field)
        {
            if (!_updatedFields.Contains(field))
                _updatedFields.Add(field);

            NewValuesEvent?.Invoke();
            OnPropertyChanged(nameof(Update));
        }

        public void DeleteUpdate(string field)
        {
            if (_updatedFields.Contains(field))
                _updatedFields.Remove(field);

            if (_updatedFields.Count == 0)
                DropRefresh();

            OnPropertyChanged(nameof(Update));
        }

        public void DropRefresh()
        {
            _updatedFields.Clear();

            SaveValuesEvent?.Invoke();
            OnPropertyChanged(nameof(Update));
        }
    }
}
