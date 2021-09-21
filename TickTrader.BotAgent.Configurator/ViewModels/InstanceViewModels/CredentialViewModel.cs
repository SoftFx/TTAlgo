using System;

namespace TickTrader.BotAgent.Configurator
{
    public class CredentialViewModel : BaseContentViewModel
    {
        private readonly string _keyLogin, _keyPassword, _keyOtpSecret;
        private readonly RefreshCounter _refreshManager;

        private CredentialModel _model;

        private DelegateCommand _generateLogin;
        private DelegateCommand _generatePassword;
        private DelegateCommand _toggleOtp, _showOtpSecret;

        public CredentialViewModel(CredentialModel model, RefreshCounter refManager = null) : base(nameof(CredentialViewModel))
        {
            _model = model;
            _refreshManager = refManager;

            _keyLogin = $"{_model.Name}{nameof(Login)}";
            _keyPassword = $"{_model.Name}{nameof(Password)}";
            _keyOtpSecret = $"{_model.Name}{nameof(_model.OtpSecret)}";
        }

        public string Name => _model.Name;

        public string Login
        {
            get => _model.Login;

            set
           {
                if (_model.Login == value)
                    return;

                _model.Login = value;
                _refreshManager?.CheckUpdate(value, _model.CurrentLogin, _keyLogin, false);

                ErrorCounter.DeleteError(_keyLogin);
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
                _refreshManager?.CheckUpdate(value, _model.CurrentPassword, _keyPassword, false);

                ErrorCounter.DeleteError(_keyPassword);
                OnPropertyChanged(nameof(Password));
            }
        }

        public bool OtpEnabled => _model.OtpEnabled;

        public string OtpStateText => _model.OtpEnabled ? "Enabled" : "Disabled";

        public override string this[string columnName]
        {
            get
            {
                var msg = "";
                try
                {
                    switch (columnName)
                    {
                        case "Login":
                            ErrorCounter.CheckStringLength(Login, 3, _keyLogin);
                            break;
                        case "Password":
                            ErrorCounter.CheckStringLength(Password, 5, _keyPassword);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    msg = ex.Message;
                }

                return msg;
            }
        }

        public DelegateCommand GeneratePassword => _generatePassword ?? (
            _generatePassword = new DelegateCommand(_ =>
            {
                _model.GeneratePassword();
                _refreshManager?.AddUpdate(_keyPassword, false);

                OnPropertyChanged(nameof(Password));
            }));

        public DelegateCommand GenerateLogin => _generateLogin ?? (
            _generateLogin = new DelegateCommand(_ =>
            {
                _model.GenerateNewLogin();
                _refreshManager?.AddUpdate(_keyLogin, false);

                OnPropertyChanged(nameof(Login));
            }));

        public DelegateCommand ToggleOtp => _toggleOtp ?? (
            _toggleOtp = new DelegateCommand(_ =>
            {
                if (_model.OtpEnabled)
                    _model.RemoveOtpSecret();
                else
                    _model.GenerateNewOtpSecret();

                _refreshManager?.CheckUpdate(_model.OtpSecret, _model.CurrentOtpSecret, _keyOtpSecret, false);

                OnPropertyChanged(nameof(OtpEnabled));
                OnPropertyChanged(nameof(OtpStateText));
            }));

        public DelegateCommand ShowOtpSecret => _showOtpSecret ?? (
            _showOtpSecret = new DelegateCommand(_ =>
            {
                var dlg = new Show2FACodeDialog(_model.OtpSecret, _model.Login);
                dlg.ShowDialog();
            }));

        public override void RefreshModel()
        {
            OnPropertyChanged(nameof(Login));
            OnPropertyChanged(nameof(Password));
            OnPropertyChanged(nameof(OtpEnabled));
            OnPropertyChanged(nameof(OtpStateText));
        }
    }
}
