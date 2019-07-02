using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TickTrader.BotAgent.Configurator
{
    public class CredentialViewModel : IContentViewModel
    {
        private CredentialModel _model;
        private RefreshManager _refreshManager;

        private DelegateCommand _generateLogin;
        private DelegateCommand _generatePassword;

        public CredentialViewModel(CredentialModel model, RefreshManager refManager = null)
        {
            _model = model;
            _refreshManager = refManager;
        }

        public string ModelDescription { get; set; }

        public string Name => _model.Name;

        public string Login
        {
            get => _model.Login;

            set
            {
                if (_model.Login == value)
                    return;

                Logger.Info($"{_model.Name}{nameof(Login)}", _model.Login, value);

                _model.Login = value;
                _refreshManager?.Refresh();

                OnPropertyChanged(nameof(Login));
            }
        }

        public string Password
        {
            get => _model.Password;

            set
            {
                if (_model.Password == value)
                    return;

                _model.Password = value;
                _refreshManager?.Refresh();

                OnPropertyChanged(nameof(Password));
            }
        }

        public DelegateCommand GeneratePassword => _generatePassword ?? (
            _generatePassword = new DelegateCommand(obj =>
            {
                _model.GeneratePassword();
                _refreshManager?.Refresh();

                OnPropertyChanged(nameof(Password));
            }));

        public DelegateCommand GenerateLogin => _generateLogin ?? (
            _generateLogin = new DelegateCommand(obj =>
            {
                _model.GenerateNewLogin();
                _refreshManager?.Refresh();

                OnPropertyChanged(nameof(Login));
            }));

        public void RefreshModel()
        {
            OnPropertyChanged(nameof(Login));
            OnPropertyChanged(nameof(Password));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
