using Machinarium.Qnil;
using Machinarium.State;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.BotTerminal.Lib;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Common.Model.Interop;

namespace TickTrader.BotTerminal
{
    internal class ConnectionManager
    {
        private Logger logger;

        private AuthStorageModel authStorage;
        private EventJournal journal;
        private Task initTask;

        public ConnectionManager(PersistModel appStorage, EventJournal journal, AlgoEnvironment algoEnv)
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
            this.authStorage = appStorage.AuthSettingsStorage;
            this.authStorage.Accounts.Updated += Storage_Changed;
            this.journal = journal;

            initTask = InitCatalog(algoEnv.Repo);

            Accounts = new ObservableCollection<AccountAuthEntry>();
            Servers = new ObservableCollection<ServerAuthEntry>();

            InitAuthData();

            var connectionOptions = new ConnectionOptions() { EnableLogs = BotTerminal.Properties.Settings.Default.EnableConnectionLogs, LogsFolder = EnvService.Instance.LogFolder };
            Connection = new ConnectionModel(connectionOptions, new DispatcherStateMachineSync());

            Connection.StateChanged += (from, to) =>
            {
                if (IsConnected(from, to))
                    journal.Info("{0}: login on {1}", Creds.Login, Creds.Server.Name);
                else if (IsUsualDisconnect(from, to))
                    journal.Info("{0}: logout from {1}", GetLast().Login, GetLast().Server.Name);
                else if (IsFailedConnection(from, to))
                    journal.Error("{0}: connect failed [{1}]", Creds.Login, Connection.LastError);
                else if (IsUnexpectedDisconnect(from, to))
                    journal.Error("{0}: connection to {1} lost [{2}]",  GetLast().Login, GetLast().Server.Name, Connection.LastError);

                logger.Debug("STATE {0}", to);

                StateChanged?.Invoke(from, to);
            };
        }

        private async Task InitCatalog(PluginCatalog catalog)
        {
            await catalog.Init();
        }

        private bool IsConnected(ConnectionModel.States from, ConnectionModel.States to)
        {
            return to == ConnectionModel.States.Online;
        }

        private bool IsUnexpectedDisconnect(ConnectionModel.States from, ConnectionModel.States to)
        {
            return from == ConnectionModel.States.Disconnecting && Connection.IsOffline && Connection.HasError;
        }

        private bool IsFailedConnection(ConnectionModel.States from, ConnectionModel.States to)
        {
            return from == ConnectionModel.States.Connecting && Connection.IsOffline && Connection.HasError && !Connection.IsReconnecting;
        }

        private bool IsUsualDisconnect(ConnectionModel.States from, ConnectionModel.States to)
        {
            return from == ConnectionModel.States.Disconnecting && to == ConnectionModel.States.Offline && !Connection.HasError;
        }

        internal void RemoveAccount(AccountAuthEntry entry)
        {
            authStorage.Remove(new AccountStorageEntry(entry.Login, entry.Password, entry.Server.Address, entry.UseSfxProtocol));
            authStorage.Save();
        }

        public ConnectionModel.States State => Connection.State;
        public ConnectionModel Connection { get; private set; }

        public AccountAuthEntry Creds { get; private set; }
        public ObservableCollection<AccountAuthEntry> Accounts { get; private set; }
        public ObservableCollection<ServerAuthEntry> Servers { get; private set; }

        public event Action CredsChanged = delegate { };
        public event Action<ConnectionModel.States, ConnectionModel.States> StateChanged = delegate { };

        public AccountAuthEntry GetLast()
        {
            if (authStorage.LastLogin != null && authStorage.LastServer != null)
                return Accounts.FirstOrDefault(a => a.Login == authStorage.LastLogin && a.Server.Address == authStorage.LastServer);
            return null;
        }

        public void TriggerConnect(AccountAuthEntry entry)
        {
            if (!entry.HasPassword)
                throw new InvalidOperationException("TriggerConnect() can be called only for accounts with saved password!");

            Connect(entry.Login, entry.Password, entry.Server.Address, entry.UseSfxProtocol, true, CancellationToken.None).Forget();
        }

        public async Task<ConnectionErrorCodes> Connect(string login, string password, string server, bool useSfx, bool savePwd, CancellationToken cToken)
        {
            logger.Debug("Connect to {0}, {1}", login, server);

            string entryPassword = savePwd ? password : null;
            var newCreds = CreateEntry(login, entryPassword, server, useSfx);

            Creds = newCreds;
            CredsChanged();

            try
            {
                await initTask;

                var code = await Connection.Connect(login, password, server, useSfx, cToken);

                if (code == ConnectionErrorCodes.None)
                    SaveLogin(newCreds);

                return code;
            }
            catch (TaskCanceledException) { return ConnectionErrorCodes.Canceled; }
        }

        public void TriggerDisconnect()
        {
            Disconnect().Forget();
        }

        public Task Disconnect()
        {
            return Connection.Disconnect();
        }

        private void InitAuthData()
        {
            AuthConfigSection cfgSection = AuthConfigSection.GetCfgSection();

            foreach (ServerElement predefinedServer in cfgSection.Servers)
                Servers.Add(new ServerAuthEntry(predefinedServer));

            foreach (var acc in authStorage.Accounts.Values)
                Accounts.Add(CreateEntry(acc));
        }

        private void SaveLogin(AccountAuthEntry entry)
        {
            authStorage.Update(new AccountStorageEntry(entry.Login, entry.Password, entry.Server.Address, entry.UseSfxProtocol));
            authStorage.UpdateLast(entry.Login, entry.Server.Address);
            authStorage.Save();
        }

        private void Storage_Changed(ListUpdateArgs<AccountStorageEntry> e)
        {
            if (e.Action == DLinqAction.Insert)
                Accounts.Add(CreateEntry(e.NewItem));
            else if (e.Action == DLinqAction.Remove)
            {
                var index = Accounts.IndexOf(a => a.Matches(e.OldItem));
                Accounts.RemoveAt(index);
            }
            else if (e.Action == DLinqAction.Replace)
            {
                var index = Accounts.IndexOf(a => a.Matches(e.OldItem));
                Accounts[index] = CreateEntry(e.NewItem);
            }
        }

        private AccountAuthEntry CreateEntry(string login, string password, string server, bool useSfx)
        {
            return CreateEntry(new AccountStorageEntry(login, password, server, useSfx));
        }

        private AccountAuthEntry CreateEntry(AccountStorageEntry record)
        {
            return new AccountAuthEntry(record, GetServer(record.ServerAddress));
        }

        private ServerAuthEntry GetServer(string address)
        {
            var server = Servers.FirstOrDefault(s => s.Address == address);

            if (server != null)
                return server;

            return new ServerAuthEntry(address);
        }
    }
}
