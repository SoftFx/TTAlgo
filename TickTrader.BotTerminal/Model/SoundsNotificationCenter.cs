using TickTrader.Algo.Account;

namespace TickTrader.BotTerminal
{
    internal sealed class SoundsNotificationCenter
    {
        private readonly SettingsStorage<PreferencesStorageModel> _persistStorage;
        private readonly ConnectionManager _connectionManager;

        private bool _enabled;


        public bool SoundEnabled
        {
            get => _enabled;

            set
            {
                if (_enabled == value)
                    return;

                _enabled = value;

                if (value)
                    _connectionManager.ConnectionStateChanged += ConnectionStateChanged;
                else
                    _connectionManager.ConnectionStateChanged -= ConnectionStateChanged;
            }
        }


        public SoundsNotificationCenter(ConnectionManager connectionModel, PersistModel persistModel)
        {
            _connectionManager = connectionModel;
            _persistStorage = persistModel.PreferencesStorage;

            _persistStorage.PropertyChanged += OnPreferencesChanged;

            SoundEnabled = _persistStorage.StorageModel.EnableSounds;
        }


        private void OnPreferencesChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_persistStorage.StorageModel.EnableSounds))
                SoundEnabled = _persistStorage.StorageModel.EnableSounds;
        }

        private void ConnectionStateChanged(ConnectionModel.States oldState, ConnectionModel.States newState)
        {
            if (newState == ConnectionModel.States.Online)
                Notify(AppSounds.Save);
            else
                if (oldState == ConnectionModel.States.Disconnecting && (newState == ConnectionModel.States.Offline || newState == ConnectionModel.States.OfflineRetry))
                    Notify(AppSounds.NegativeLong);
        }

        private void Notify(IPlayable sound)
        {
            if (SoundEnabled)
                sound.Play();
        }
    }
}
