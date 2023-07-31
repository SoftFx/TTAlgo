using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using TickTrader.Algo.AppCommon;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Server;
using TickTrader.Algo.Server.Persistence;
using TickTrader.BotAgent.WebAdmin.Server.Extensions;

namespace TickTrader.BotAgent.BA.Models
{
    [DataContract(Name = "server.config", Namespace = "")]
    public class ServerModel : IBotAgent
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<ServerModel>();

        private static readonly EnvService envService = new(AppInfoProvider.DataPath);
        private static readonly string cfgFilePath = Path.Combine(envService.AppFolder, "server.config.xml");

        [DataMember(Name = "accounts")]
        private List<ClientModel> _accounts = new();

        private LocalAlgoServer _algoServer = new();

        public static EnvService Environment => envService;

        public IAlgoServerLocal Server => _algoServer;

        public static string GetWorkingFolderFor(string botId)
        {
            return Path.Combine(Environment.AlgoWorkingFolder, PathHelper.Escape(botId));
        }

        public async Task InitAsync(IConfiguration config)
        {
            var settings = new AlgoServerSettings();
            var monitoringSettings = config.GetMonitoringSettings();

            settings.DataFolder = AppInfoProvider.DataPath;
            settings.EnableAccountLogs = config.GetFdkSettings().EnableLogs;
            settings.EnableAutoUpdate = true;

            settings.HostSettings.RuntimeSettings.RuntimeExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "runtime", "TickTrader.Algo.RuntimeV1Host.exe");
            settings.HostSettings.RuntimeSettings.EnableDevMode = config.GetAlgoSettings().EnableDevMode;
            settings.HostSettings.PkgStorage.AddLocation(SharedConstants.LocalRepositoryId, envService.AlgoRepositoryFolder);
            settings.HostSettings.PkgStorage.UploadLocationId = SharedConstants.LocalRepositoryId;

            settings.MonitoringSettings.QuoteMonitoring = new()
            {
                EnableMonitoring = monitoringSettings.QuoteMonitoring.EnableMonitoring,
                AccetableQuoteDelay = monitoringSettings.QuoteMonitoring.AccetableQuoteDelay,
                AlertsDelay = monitoringSettings.QuoteMonitoring.AlertsDelay,
                SaveOnDisk = monitoringSettings.QuoteMonitoring.SaveOnDisk,
            };

            await _algoServer.Init(settings);

            if (await _algoServer.NeedLegacyState())
            {
                await _algoServer.LoadLegacyState(BuildServerSavedState());
            }

            await _algoServer.Start();
        }

        public async Task ShutdownAsync()
        {
            _logger.Debug("ServerModel is shutting down...");

            await _algoServer.Stop();
        }


        private ServerSavedState BuildServerSavedState()
        {
            var state = new ServerSavedState();

            foreach (var acc in _accounts)
            {
                var server = acc.Address;
                var userId = acc.Username;
                var accState = new AccountSavedState
                {
                    Id = AccountId.Pack(server, userId),
                    UserId = userId,
                    Server = server,
                    DisplayName = string.IsNullOrEmpty(acc.DisplayName) ? $"{server} - {userId}" : acc.DisplayName,
                };

                accState.PackCreds(new AccountCreds(acc.Password));

                state.Accounts.Add(accState.Id, accState);

                acc.AddPluginsSavedStates(state, accState.Id);
            }

            return state;
        }

        #region Serialization

        private void Save()
        {
            try
            {
                var settings = new XmlWriterSettings { Indent = true };
                DataContractSerializer serializer = new DataContractSerializer(typeof(ServerModel));
                using (var writer = XmlWriter.Create(cfgFilePath, settings))
                    serializer.WriteObject(writer, this);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to save config file! {ex.Message}");
            }
        }

        public static ServerModel Load()
        {
            ServerModel instance;

            try
            {
                using (var stream = File.OpenRead(cfgFilePath))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(ServerModel));
                    instance = (ServerModel)serializer.ReadObject(stream);
                    instance._algoServer = new LocalAlgoServer();
                }
            }
            catch (FileNotFoundException)
            {
                instance = new ServerModel();
            }

            return instance;
        }

        #endregion
    }
}
