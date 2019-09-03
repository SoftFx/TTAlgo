using System.Collections.Generic;

namespace TickTrader.BotAgent.Configurator
{
    public class RefreshCounter : BaseViewModel
    {
        private SortedSet<string> _updatedFields;

        public delegate void ConfigurationStateChanged();

        public event ConfigurationStateChanged ChangeValuesEvent;

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

            Refresh();
        }

        public void DeleteUpdate(string field)
        {
            if (_updatedFields.Contains(field))
                _updatedFields.Remove(field);

            Refresh();
        }

        public void DropRefresh()
        {
            _updatedFields.Clear();
            Refresh();
        }

        private void Refresh()
        {
            ChangeValuesEvent?.Invoke();
            OnPropertyChanged(nameof(Update));
        }
    }
}
