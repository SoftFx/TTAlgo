using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TickTrader.BotAgent.Configurator
{
    public class ServerViewModel : BaseViewModel, IContentViewModel
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private NewUrlWindow _addUrlWnd;

        private DelegateCommand _removeUrls;
        private DelegateCommand _generateSecretKey;
        private DelegateCommand _addUrlDialog;
        private DelegateCommand _addNewUri;
        private DelegateCommand _modifyUri;

        private ServerModel _model;
        private RefreshManager _refreshManager;

        private Uri _oldUri;

        public NewUriViewModel NewUri { get; set; }

        public ServerViewModel(ServerModel model, RefreshManager refManager = null)
        {
            _model = model;
            _refreshManager = refManager;

            ResetSetting();
        }

        public ObservableCollection<Uri> Urls { get; private set; }
        public ObservableCollection<string> Hosts => new ObservableCollection<string>(Urls.Select(u => u.Host).Distinct());

        public string SecretKey => _model.SecretKey;

        public string ModelDescription { get; set; }

        public string UrlsDescription { get; set; }

        public string SecretKeyDescription { get; set; }

        public DelegateCommand ModifyUri => _modifyUri ?? (
            _modifyUri = new DelegateCommand(obj =>
            {
                var uri = (Uri)obj;
                _oldUri = uri;
                NewUri = new NewUriViewModel(uri.Scheme, uri.Host, uri.Port);
                (_addUrlWnd = new NewUrlWindow(this)).ShowDialog();
            }));

        public DelegateCommand SaveNewUri => _addNewUri ?? (
            _addNewUri = new DelegateCommand(obj =>
            {
                var uri = NewUri.GetUri();

                if (!Urls.Contains(uri))
                {
                    Urls.Add(uri);
                    _model.Urls.Add(uri);

                    Urls.Remove(_oldUri);
                    _model.Urls.Remove(_oldUri);
                    _refreshManager?.Refresh();
                    _logger.Info($"{nameof(ServerViewModel)} new url was added: {uri}");
                }

                _addUrlWnd.DialogResult = true;
            }));

        public DelegateCommand RemoveUrls => _removeUrls ?? (
                    _removeUrls = new DelegateCommand(obj =>
                    {
                        foreach (Uri u in ((IList<object>)obj).ToList())
                        {
                            Urls.Remove(u);
                            _model.Urls.Remove(u);
                        }

                        _logger.Info($"{nameof(ServerViewModel)} select urls was removed");

                        _refreshManager?.Refresh();
                    }));

        public DelegateCommand GenerateSecretKey => _generateSecretKey ?? (
            _generateSecretKey = new DelegateCommand(obj =>
            {
                _model.GenerateSecretKey();
                _refreshManager?.Refresh();

                OnPropertyChanged(nameof(SecretKey));
            }));

        public DelegateCommand AddUrlDialog => _addUrlDialog ?? (
            _addUrlDialog = new DelegateCommand(obj =>
            {
                NewUri = new NewUriViewModel();
                _oldUri = null;
                (_addUrlWnd = new NewUrlWindow(this)).ShowDialog();
            }));

        public void ResetSetting()
        {
            NewUri = new NewUriViewModel();
            Urls = new ObservableCollection<Uri>(_model.Urls);
        }

        public void RefreshModel()
        {
            OnPropertyChanged(nameof(Urls));
            OnPropertyChanged(nameof(SecretKey));
        }
    }

    public class NewUriViewModel
    {
        private const string specialSymbols = "$-_.+ !*'()";

        private string _host = "localhost";
        private int _port;

        public List<string> TypesOfScheme { get; } = new List<string>() { "https", "http" };

        public NewUriViewModel() { }

        public NewUriViewModel(string scheme, string host, int port)
        {
            Scheme = scheme;
            Host = host;
            Port = port;
        }

        public string Scheme { get; set; } = "https";

        public string Host
        {
            get => _host;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("This field is required");

                foreach (var c in value)
                {
                    if (!char.IsLetterOrDigit(c) && specialSymbols.IndexOf(c) == -1)
                        throw new ArgumentException($"An invalid character {c} was found");
                }

                _host = value;
            }
        }

        public int Port
        {
            get => _port;
            set
            {
                if (value < 0 || value > (1 << 16))
                    throw new ArgumentException($"Port must be between {0} to {1 << 16}");

                _port = value;
            }
        }

        public Uri GetUri() => new UriBuilder(Scheme, Host, Port).Uri;
    }
}
