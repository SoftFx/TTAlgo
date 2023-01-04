using Machinarium.Qnil;
using System;
using System.IO;
using System.Linq;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Indicators.Trend.MovingAverage;
using TickTrader.Algo.Server;
using TickTrader.Algo.Server.Persistence;

namespace TickTrader.BotTerminal
{
    internal class LocalAlgoAgent
    {
        public const string LocalAgentName = "AlgoTerminal";

        public static AlgoServerSettings GetSettings()
        {
            var settings = new AlgoServerSettings();
            settings.HostSettings.DataFolder = AppDomain.CurrentDomain.BaseDirectory;
            settings.EnableAccountLogs = Properties.Settings.Default.EnableConnectionLogs;
            settings.EnableIndicatorHost = true;
            settings.HostSettings.RuntimeSettings.RuntimeExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "runtime", "TickTrader.Algo.RuntimeV1Host.exe");
            settings.HostSettings.RuntimeSettings.EnableDevMode = Properties.Settings.Default.EnableDevMode;
            settings.HostSettings.PkgStorage.Assemblies.Add(typeof(MovingAverage).Assembly);
            settings.HostSettings.PkgStorage.AddLocation(SharedConstants.LocalRepositoryId, EnvService.Instance.AlgoRepositoryFolder);
            settings.HostSettings.PkgStorage.UploadLocationId = SharedConstants.LocalRepositoryId;
            if (EnvService.Instance.AlgoCommonRepositoryFolder != null)
                settings.HostSettings.PkgStorage.AddLocation(SharedConstants.CommonRepositoryId, EnvService.Instance.AlgoCommonRepositoryFolder);

            return settings;
        }

        public static ServerSavedState BuildServerSavedState(PersistModel model)
        {
            var state = new ServerSavedState();

            state = AddAccountsSavedStates(state, model);

            return state;
        }

        private static ServerSavedState AddAccountsSavedStates(ServerSavedState state, PersistModel model)
        {
            foreach (var acc in model.AuthSettingsStorage.Accounts.Values)
            {
                var accState = new AccountSavedState
                {
                    Id = AccountId.Pack(acc.ServerAddress, acc.Login),
                    UserId = acc.Login,
                    Server = acc.ServerAddress,
                    DisplayName = $"{acc.ServerAddress} - {acc.Login}",
                };

                if (acc.HasPassword)
                    accState.PackCreds(new AccountCreds(acc.Password));

                state.Accounts.Add(accState.Id, accState);

                AddPluginsSavedStates(state, model, acc.ServerAddress, acc.Login);
            }

            return state;
        }

        private static ServerSavedState AddPluginsSavedStates(ServerSavedState state, PersistModel model, string accServer, string accLogin)
        {
            //var accLogin = model.AuthSettingsStorage.LastLogin;
            //var accServer = model.AuthSettingsStorage.LastServer;

            if (string.IsNullOrEmpty(accLogin) || string.IsNullOrEmpty(accServer))
                return state;

            var profile = model.ProfileManager.LoadCachedProfileOnce(accServer, accLogin);

            if (profile?.Bots == null)
                return state;

            foreach (var config in profile.Bots.Select(u => u.Config))
            {
                var pluginState = new PluginSavedState
                {
                    Id = config.InstanceId,
                    AccountId = AccountId.Pack(accServer, accLogin),
                    IsRunning = false,
                };

                pluginState.PackConfig(config.ToDomain());

                state.Plugins.Add(pluginState.Id, pluginState);
            }

            return state;
        }
    }
}
