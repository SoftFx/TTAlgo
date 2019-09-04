using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TickTrader.BotAgent.Configurator
{
    public class ServerViewModel : BaseContentViewModel
    {
        private readonly RefreshCounter _refreshManager;
        private readonly string _urlsKey = $"{nameof(Urls)}";

        private NewUrlWindow _urlWindow;

        private DelegateCommand _removeUrls;
        private DelegateCommand _generateSecretKey;
        private DelegateCommand _saveNewUri;
        private DelegateCommand _openUriWindow;
        private DelegateCommand _closeUriWindow;

        private ServerModel _model;      

        public string Title => CurrentUri?.OldUri != null ? "Modify URL" : "Add URL";

        public string SecretKey => _model.SecretKey;

        public string UrlsDescription { get; set; }

        public string SecretKeyDescription { get; set; }

        public UriViewModel CurrentUri { get; private set; }

        public ObservableCollection<UriWithValidation> Urls
        {
            get
            {
                var urls = _model.Urls.Select(u => new UriWithValidation(u, _model.PortsManager));

                int busyPortsCount = urls.Where(u => u.HasWarning).Count();

                if (busyPortsCount != 0)
                    ErrorCounter.AddWarning(_urlsKey);
                else
                    ErrorCounter.DeleteWarning(_urlsKey);

                return new ObservableCollection<UriWithValidation>(urls);
            }
        }


        public ServerViewModel(ServerModel model, RefreshCounter refManager = null) : base(nameof(ServerViewModel))
        {
            _model = model;
            _refreshManager = refManager;

            RefreshModel();
        }

        public DelegateCommand OpenUriWindow => _openUriWindow ?? (
            _openUriWindow = new DelegateCommand(obj =>
            {
                CurrentUri.SetOldUri(obj);
                (_urlWindow = new NewUrlWindow(this)).ShowDialog();
            }));

        public DelegateCommand CloseUriWindow => _closeUriWindow ?? (
            _closeUriWindow = new DelegateCommand(obj =>
            {
                _urlWindow.Close();
            }));

        public DelegateCommand SaveUri => _saveNewUri ?? (
            _saveNewUri = new DelegateCommand(obj =>
            {
                if (CurrentUri.OldUri != null)
                    _model.Urls[Urls.IndexOf(CurrentUri.OldUri)] = CurrentUri.GetUri();
                else
                    _model.Urls.Add(CurrentUri.GetUri());

                UpdateRefreshAndErrors();

                _urlWindow.DialogResult = true;
            }));

        public DelegateCommand RemoveUrls => _removeUrls ?? (
            _removeUrls = new DelegateCommand(obj =>
            {
                if (!MessageBoxManager.YesNoBoxQuestion("Are you sure to delete the selected urls?", "Urls deletion"))
                    return;

                foreach (Uri u in ((IList<object>)obj).ToList())
                    _model.Urls.Remove(u);

                UpdateRefreshAndErrors();
            }));

        public DelegateCommand GenerateSecretKey => _generateSecretKey ?? (
            _generateSecretKey = new DelegateCommand(obj =>
            {
                _model.GenerateSecretKey();
                _refreshManager?.AddUpdate(nameof(SecretKey));

                OnPropertyChanged(nameof(SecretKey));
            }));


        public override void RefreshModel()
        {
            CurrentUri = new UriViewModel(_model.PortsManager, _model.Urls);

            UpdateRefreshAndErrors();
            OnPropertyChanged(nameof(SecretKey));
        }

        private void UpdateRefreshAndErrors()
        {
            if (!_model.CheckOnDefaultValue())
                _refreshManager?.AddUpdate(_urlsKey);
            else
                _refreshManager?.DeleteUpdate(_urlsKey);

            OnPropertyChanged(nameof(Urls));
        }
    }

    public class UriWithValidation : Uri
    {
        public bool HasWarning { get; private set; }

        public string Warning { get; private set; }

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
                HasWarning = true;
                Warning = ex.Message;
            }
        }
    }
}
