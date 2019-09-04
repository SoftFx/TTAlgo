using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TickTrader.BotAgent.Configurator
{
    public class UriViewModel : BaseViewModelWithValidations
    {
        private const string specialSymbols = "$-_.+ !*'()";
        private const int MinPort = 1, MaxPort = 1 << 16;

        private readonly List<Uri> _urls;
        private readonly PortsManager _portsManager;

        private int _port = 0;

        public List<string> TypesOfScheme => new List<string>() { "https", "http" };

        public IEnumerable<string> Hosts => _urls.Select(u => u.Host).Distinct();

        public bool IsFreePort { get; private set; }
        
        public UriWithValidation OldUri { get; private set; }

        public string WarningMessage { get; private set; }

        public string Scheme { get; set; }

        public string Host { get; set; }

        public string Port
        {
            get => _port.ToString();
            set
            {
                IsFreePort = true;
                OnPropertyChanged(nameof(IsFreePort));

                _port = int.Parse(value);
            }
        }
        
        public UriViewModel(PortsManager manager, List<Uri> urls)
        {
            _portsManager = manager;
            _urls = urls;
        }

        public void SetOldUri(object obj)
        {
            OldUri = (UriWithValidation)obj;

            if (OldUri != null)
            {
                
                Scheme = OldUri.Scheme;
                Host = OldUri.Host;
                _port = OldUri.Port;
            }
            else
            {
                OldUri = null;
                Scheme = "https";
                Host = "localhost";
                _port = 0;
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
                            {
                                if (string.IsNullOrEmpty(Host))
                                    throw new ArgumentException("This field is required");

                                foreach (var c in Host)
                                {
                                    if (!char.IsLetterOrDigit(c) && specialSymbols.IndexOf(c) == -1)
                                        throw new ArgumentException($"An invalid character {c} was found");
                                }
                                break;
                            }
                        case "Port":
                            {
                                if (_port < MinPort || _port > MaxPort)
                                    throw new ArgumentException($"Port must be between {MinPort} to {MaxPort}");
                                break;
                            }
                    }

                    ExistanceCheck();
                    _portsManager.CheckPort(_port, OldUri?.Port ?? 0, Host);
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

            if (_urls.Where(u => u.Host == Host && u.Port == _port).Count() > 0)
                throw new Exception("Current url already exists!");
        }

        public Uri GetUri() => new UriBuilder(Scheme, Host, _port).Uri;
    }
}
