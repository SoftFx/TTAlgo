using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TickTrader.BotAgent.Configurator.Properties;

namespace TickTrader.BotAgent.Configurator
{
    public class UriViewModel : BaseViewModelWithValidations
    {
        private const string SpecialSymbols = "$-_.+ !*'()";
        private const int MinPort = 1, MaxPort = (1 << 16) - 1;

        private readonly List<string> _correctHosts = new List<string>()
        {
            IPAddress.Any.ToString(),
            $"[{IPAddress.IPv6Any.ToString()}]",
            IPAddress.Loopback.ToString(),
            $"[{IPAddress.IPv6Loopback.ToString()}]"
        };

        private readonly PortsManager _portsManager;
        private readonly ProtocolViewModel _protocolManager;

        private List<Uri> _urls;

        private int _port = 0;
        private string _host = string.Empty;

        public List<string> TypesOfScheme => new List<string>() { "https", "http" };

        public IEnumerable<string> Hosts => _urls.Select(u => u.Host).Distinct();

        public bool IsFreePort { get; private set; }

        public UriWithValidation OldUri { get; private set; }

        public string WarningMessage { get; private set; }

        public string Scheme { get; set; }

        public string Host
        {
            get => _host;
            set
            {
                _host = value;

                OnPropertyChanged(nameof(Port));
            }
        }

        public string Port
        {
            get => _port.ToString();
            set
            {
                IsFreePort = true;
                OnPropertyChanged(nameof(IsFreePort));

                _port = int.Parse(value);

                OnPropertyChanged(nameof(Host));
            }
        }

        public UriViewModel(PortsManager manager, List<Uri> urls, ProtocolViewModel protocolManager)
        {
            _protocolManager = protocolManager;
            _portsManager = manager;
            _urls = urls;
        }

        public void SetOldUri(object obj, List<Uri> urls)
        {
            OldUri = (UriWithValidation)obj;
            _urls = urls;

            if (OldUri != null)
            {

                Scheme = OldUri.Scheme;
                _port = OldUri.Port;
                Host = OldUri.Host;
            }
            else
            {
                OldUri = null;
                Scheme = "https";
                _port = 0;
                Host = "localhost";
            }
        }

        public override string this[string columnName]
        {
            get
            {
                IsFreePort = true;
                var msg = "";

                try
                {
                    switch (columnName)
                    {
                        case "Host":
                            if (string.IsNullOrEmpty(Host))
                                throw new ArgumentException(Resources.RequiredFieldEx);

                            if (GetUri().HostNameType != UriHostNameType.Dns)
                            {
                                if (!_correctHosts.Contains(Host))
                                    throw new ArgumentException(Resources.InvalidHostEx);
                            }
                            else
                            {
                                foreach (var c in Host)
                                    if (!char.IsLetterOrDigit(c) && SpecialSymbols.IndexOf(c) == -1)
                                        throw new ArgumentException($"{Resources.InvalidCh_Ex} {c} {Resources._InvalidChEx}");
                            }
                            break;

                        case "Port":
                            if (_port < MinPort || _port > MaxPort)
                                throw new ArgumentException($"{Resources.PortRangeEx} {MinPort} to {MaxPort}");
                            break;
                    }

                    ExistanceCheck();
                    _portsManager.CheckPort(_port, Host);
                }
                catch (WarningException ex)
                {
                    IsFreePort = false;
                    WarningMessage = ex.Message;
                }
                catch (Exception ex)
                {
                    msg = ex.Message;
                }

                OnPropertyChanged(nameof(IsFreePort));
                OnPropertyChanged(nameof(WarningMessage));
                return msg;
            }
        }

        private void ExistanceCheck()
        {
            if (OldUri != null && OldUri.Host == Host && OldUri.Port == _port)
                return;

            var copyUrls = new List<Uri>(_urls);

            if (OldUri != null)
                copyUrls.Remove(OldUri);

            var encodedUrls = UriChecker.GetEncodedUrls(copyUrls);

            var uri = GetUri();

            if (uri.HostNameType == UriHostNameType.Dns)
            {
                var hosts = UriChecker.GetEncodedDnsHosts(uri);

                if (encodedUrls.Where(u => u.Host == hosts.Item1 && u.Port == uri.Port).Count() > 0)
                    throw new ArgumentException(Resources.ExistingUrlEx);

                if (encodedUrls.Where(u => u.Host == hosts.Item2 && u.Port == uri.Port).Count() > 0)
                    throw new ArgumentException(Resources.ExistingUrlEx);
            }
            else
            {
                if (encodedUrls.Where(u => u.Host == Host && u.Port == _port).Count() > 0)
                    throw new ArgumentException(Resources.ExistingUrlEx);
            }

            UriChecker.CompareUriWithWarnings(_protocolManager.ListeningUri, uri, Resources.EqualToListeningPortEx);
        }

        public Uri GetUri() => new UriBuilder(Scheme, Host, _port).Uri;
    }
}
