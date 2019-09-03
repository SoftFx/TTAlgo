using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TickTrader.BotAgent.Configurator
{
    public class ServerViewModel : BaseContentViewModel
    {
        private readonly RefreshCounter _refreshManager;

        private NewUrlWindow _urlWindow;

        private DelegateCommand _removeUrls;
        private DelegateCommand _generateSecretKey;
        private DelegateCommand _openNewUrlWindow;
        private DelegateCommand _saveNewUri;
        private DelegateCommand _openModifyUrlWindow;
        private DelegateCommand _closeWindow;

        private ServerModel _model;
        private readonly string _urlsKey = $"{nameof(Urls)}";


        public ServerViewModel(ServerModel model, RefreshCounter refManager = null) : base(nameof(ServerViewModel))
        {
            _model = model;
            _refreshManager = refManager;

            RefreshModel();
        }

        public ObservableCollection<string> Hosts => new ObservableCollection<string>(Urls.Select(u => u.Host).Distinct());

        public string Title => CurrentUri?.OldUri != null ? "Modify URL" : "Add URL";

        public string SecretKey => _model.SecretKey;

        public string UrlsDescription { get; set; }

        public string SecretKeyDescription { get; set; }

        public bool ModifyWindow { get; private set; }

        public UriViewModel CurrentUri { get; private set; }

        public ObservableCollection<UriWithValidation> Urls { get; private set; }

        public DelegateCommand ModifyUri => _openModifyUrlWindow ?? (
            _openModifyUrlWindow = new DelegateCommand(obj =>
            {
                if (obj == null)
                {
                    MessageBoxManager.OkError("Please select an url to modify");
                    return;
                }

                CurrentUri = new UriViewModel((UriWithValidation)obj, _model.PortsManager, _model.Urls);
                (_urlWindow = new NewUrlWindow(this)).ShowDialog();
            }));

        public DelegateCommand AddUri => _openNewUrlWindow ?? (
            _openNewUrlWindow = new DelegateCommand(obj =>
            {
                CurrentUri = new UriViewModel(_model.PortsManager, _model.Urls);
                (_urlWindow = new NewUrlWindow(this)).ShowDialog();
            }));

        public DelegateCommand SaveUri => _saveNewUri ?? (
            _saveNewUri = new DelegateCommand(obj =>
            {
                var uri = CurrentUri.GetUri();

                if (!Urls.Contains(uri))
                {
                    if (CurrentUri.OldUri != null)
                        ModifyUriMethod();
                    else
                        AddUriMethod(CurrentUri.GetUri());

                    UpdateRefreshAndErrors();
                }
                else
                    if (CurrentUri.OldUri == null)
                    MessageBoxManager.WarningBox("Сurrent url already exists");

                _urlWindow.DialogResult = true;
            }));

        public DelegateCommand RemoveUrls => _removeUrls ?? (
            _removeUrls = new DelegateCommand(obj =>
            {
                if (!MessageBoxManager.YesNoBoxQuestion("Are you sure to delete the selected urls?", "Urls deletion"))
                    return;

                foreach (Uri u in ((IList<object>)obj).ToList())
                    RemoveUriMethod(u);

                UpdateRefreshAndErrors();
            }));

        public DelegateCommand GenerateSecretKey => _generateSecretKey ?? (
            _generateSecretKey = new DelegateCommand(obj =>
            {
                _model.GenerateSecretKey();
                _refreshManager?.AddUpdate(nameof(SecretKey));

                OnPropertyChanged(nameof(SecretKey));
            }));

        public DelegateCommand CloseWindow => _closeWindow ?? (
            _closeWindow = new DelegateCommand(obj =>
            {
                _urlWindow.Close();
            }));

        public override void RefreshModel()
        {
            CurrentUri = new UriViewModel(_model.PortsManager, _model.Urls);
            Urls = new ObservableCollection<UriWithValidation>(_model.Urls.Select(u => new UriWithValidation(u, _model.PortsManager)));
            UpdateRefreshAndErrors();

            OnPropertyChanged(nameof(Urls));
            OnPropertyChanged(nameof(SecretKey));
        }

        private void AddUriMethod(Uri uri)
        {
            Urls.Add(new UriWithValidation(uri, _model.PortsManager));
            _model.Urls.Add(uri);
        }

        private void RemoveUriMethod(Uri uri)
        {
            if (uri == null)
                return;

            Urls.Remove(new UriWithValidation(uri, _model.PortsManager));
            _model.Urls.Remove(uri);
        }

        private void ModifyUriMethod()
        {
            var index = Urls.IndexOf(CurrentUri.OldUri);

            Urls[index] = new UriWithValidation(CurrentUri.GetUri(), _model.PortsManager);
            _model.Urls[index] = CurrentUri.GetUri();
        }

        private void UpdateRefreshAndErrors()
        {
            if (!_model.CheckOnDefaultValue())
                _refreshManager?.AddUpdate(nameof(_model.Urls));
            else
                _refreshManager?.DeleteUpdate(nameof(_model.Urls));

            int busyPortsCount = Urls.Where(u => u.HasError).Count();

            if (busyPortsCount != 0)
                ErrorCounter.AddWarning(_urlsKey);
            else
                ErrorCounter.DeleteWarning(_urlsKey);
        }
    }

    public class UriWithValidation : Uri
    {
        public bool HasError { get; private set; }

        public string Error { get; private set; }

        public UriWithValidation(Uri uri, PortsManager manager) : this(uri.ToString(), manager)
        { }

        public UriWithValidation(string str, PortsManager manager) : base(str)
        {
            try
            {
                manager.CheckPort(Port, Port, Host);
            }
            catch (WarningException ex)
            {
                HasError = true;
                Error = ex.Message;
            }
        }
    }

    public class UriViewModel : BaseViewModel
    {
        private const string specialSymbols = "$-_.+ !*'()";

        private readonly List<Uri> _urls;
        private readonly PortsManager _portsManager;

        private string _host = "localhost";
        private int _port;

        public List<string> TypesOfScheme { get; } = new List<string>() { "https", "http" };

        public UriWithValidation OldUri { get; private set; }

        public bool IsFreePort { get; private set; } = true;

        public string WarningMessage { get; private set; }

        public UriViewModel(PortsManager manager, List<Uri> urls)
        {
            _portsManager = manager;
            _urls = urls;
        }

        public UriViewModel(UriWithValidation old, PortsManager manager, List<Uri> urls) : this(manager, urls)
        {
            OldUri = old;
            Scheme = old.Scheme;
            Host = old.Host;
            Port = old.Port;
        }

        public string Scheme { get; set; } = "https";

        public string Host
        {
            get => _host;
            set
            {
                _host = value;

                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("This field is required");

                foreach (var c in value)
                {
                    if (!char.IsLetterOrDigit(c) && specialSymbols.IndexOf(c) == -1)
                        throw new ArgumentException($"An invalid character {c} was found");
                }

                ExistanceCheck();

                Port = _port;

                OnPropertyChanged(nameof(Host));
                OnPropertyChanged(nameof(Port));
            }
        }

        public int Port
        {
            get => _port;
            set
            {
                _port = value;

                IsFreePort = true;

                if (value < 0 || value > (1 << 16))
                    throw new ArgumentException($"Port must be between {0} to {1 << 16}");

                ExistanceCheck();

                try
                {
                    _portsManager.CheckPort(value, OldUri?.Port ?? -1, Host);
                }
                catch (WarningException ex)
                {
                    IsFreePort = false;
                    WarningMessage = ex.Message;
                }

                OnPropertyChanged(nameof(Port));
                OnPropertyChanged(nameof(IsFreePort));
            }
        }

        private void ExistanceCheck()
        {
            bool status = OldUri != null ? GetUri() != OldUri : true;

            if (status && _urls.Contains(GetUri()))
                throw new Exception("This url already exists");
        }

        public Uri GetUri() => new UriBuilder(Scheme, Host, Port).Uri;
    }
}
