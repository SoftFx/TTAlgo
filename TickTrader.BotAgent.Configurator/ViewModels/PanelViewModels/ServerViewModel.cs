using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TickTrader.BotAgent.Configurator
{
    public class ServerViewModel : BaseContentViewModel
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly RefreshCounter _refreshManager;

        private NewUrlWindow _urlWindow;

        private DelegateCommand _removeUrls;
        private DelegateCommand _generateSecretKey;
        private DelegateCommand _openNewUrlWindow;
        private DelegateCommand _saveNewUri;
        private DelegateCommand _openModifyUrlWindow;
        private DelegateCommand _closeWindow;

        private ServerModel _model;

        public UriViewModel CurrentUri { get; set; }

        public string Title => CurrentUri?.OldUri != null ? "Modify URL" : "Add URL";

        public bool ModifyWindow { get; private set; }

        public ServerViewModel(ServerModel model, RefreshCounter refManager = null) : base(nameof(ServerViewModel))
        {
            _model = model;
            _refreshManager = refManager;

            RefreshModel();
        }

        public ObservableCollection<Uri> Urls { get; private set; }
        public ObservableCollection<string> Hosts => new ObservableCollection<string>(Urls.Select(u => u.Host).Distinct());

        public string SecretKey => _model.SecretKey;

        public string UrlsDescription { get; set; }

        public string SecretKeyDescription { get; set; }

        public DelegateCommand ModifyUri => _openModifyUrlWindow ?? (
            _openModifyUrlWindow = new DelegateCommand(obj =>
            {
                if (obj == null)
                {
                    MessageBoxManager.ErrorBox("Please select an url to modify");
                    return;
                }

                CurrentUri = new UriViewModel((Uri)obj, _model.PortsManager);
                (_urlWindow = new NewUrlWindow(this)).ShowDialog();
            }));

        public DelegateCommand AddUri => _openNewUrlWindow ?? (
            _openNewUrlWindow = new DelegateCommand(obj =>
            {
                CurrentUri = new UriViewModel(_model.PortsManager);
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

                    _refreshManager?.AddUpdate(nameof(SaveUri));
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

                _refreshManager?.AddUpdate(nameof(RemoveUrls));
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
            CurrentUri = new UriViewModel(_model.PortsManager);
            Urls = new ObservableCollection<Uri>(_model.Urls);

            OnPropertyChanged(nameof(Urls));
            OnPropertyChanged(nameof(SecretKey));
        }

        private void AddUriMethod(Uri uri)
        {
            Urls.Add(uri);
            _model.Urls.Add(uri);

            _logger.Info($"New url was added: {uri}");
        }

        private void RemoveUriMethod(Uri uri)
        {
            if (uri == null)
                return;

            Urls.Remove(uri);
            _model.Urls.Remove(uri);

            _logger.Info($"Url was removed: {uri.ToString()}");
        }

        private void ModifyUriMethod()
        {
            var index = Urls.IndexOf(CurrentUri.OldUri);

            Urls[index] = CurrentUri.GetUri();
            _model.Urls[index] = CurrentUri.GetUri();

            _logger.Info($"Url was changed {CurrentUri.OldUri.ToString()} to {CurrentUri.GetUri()}");
        }
    }

    public class UriViewModel : BaseContentViewModel
    {
        private const string specialSymbols = "$-_.+ !*'()";

        private PortsManager _portsManager;

        private string _host = "localhost";
        private int _port;

        public List<string> TypesOfScheme { get; } = new List<string>() { "https", "http" };

        public Uri OldUri { get; private set; }

        public UriViewModel(PortsManager manager)
        {
            _portsManager = manager;
        }

        public UriViewModel(Uri old, PortsManager manager)
        {
            _portsManager = manager;

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

                if (value < 0 || value > (1 << 16))
                    throw new ArgumentException($"Port must be between {0} to {1 << 16}");

                _portsManager.CheckPort(value, Host, OldUri?.Port ?? -1);
            }
        }

        public Uri GetUri() => new UriBuilder(Scheme, Host, Port).Uri;
    }
}
