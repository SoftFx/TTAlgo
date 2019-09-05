using System.Collections.Generic;

namespace TickTrader.BotAgent.Configurator
{
    public class RefreshCounter : BaseViewModel
    {
        private SortedSet<string> _updatedFields;
        private SortedSet<string> _restartFields;

        public delegate void ConfigurationStateChanged();

        public event ConfigurationStateChanged ChangeValuesEvent;

        public bool Update => _updatedFields.Count > 0;

        public bool Restart => _restartFields.Count > 0;

        public RefreshCounter()
        {
            _updatedFields = new SortedSet<string>();
            _restartFields = new SortedSet<string>();
        }

        public void CheckUpdate(string newValue, string oldValue, string field, bool restart = true)
        {
            if (newValue != oldValue)
                AddUpdate(field, restart);
            else
                DeleteUpdate(field);
        }

        public void AddUpdate(string field, bool restart = true)
        {
            if (!_updatedFields.Contains(field))
                _updatedFields.Add(field);

            if (!_restartFields.Contains(field) && restart)
                _restartFields.Add(field);

            Refresh();
        }

        public void DeleteUpdate(string field)
        {
            if (_updatedFields.Contains(field))
                _updatedFields.Remove(field);

            if (_restartFields.Contains(field))
                _restartFields.Remove(field);

            Refresh();
        }

        public void DropRefresh()
        {
            _updatedFields.Clear();
            Refresh();
        }

        public void DropRestart()
        {
            _restartFields.Clear();
            Refresh();
        }

        public void AllDrop()
        {
            _updatedFields.Clear();
            _restartFields.Clear();
            Refresh();
        }

        private void Refresh()
        {
            ChangeValuesEvent?.Invoke();
            OnPropertyChanged(nameof(Update));
            OnPropertyChanged(nameof(Restart));
        }
    }
}
