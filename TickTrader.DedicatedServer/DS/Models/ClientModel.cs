using Machinarium.ActorModel;
using Machinarium.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;

namespace TickTrader.DedicatedServer.DS.Models
{
    [DataContract]
    public class ClientModel
    {
        public enum States { Offline, Connecting, Online, Disconnecting }
        private enum Events { Connected, ConnectFailed, CredsModified, LostConnection, DoneDisconnecting }

        private StateMachine<States> stateControl = new StateMachine<States>();
        private CancellationTokenSource connectCancellation;
        private TaskCompletionSource<ConnectionErrorCodes> resultEvent;

        private bool testRequested;
        private ConnectionErrorCodes connectResult;

        public ClientModel()
        {
            Connection = new ConnectionModel(null, new ConnectionOptions() { EnableFixLogs = false });
            Connection.State.StateChanged += Connection_StateChanged;

            stateControl.AddTransition(States.Offline, () => testRequested, States.Connecting);
            stateControl.AddTransition(States.Connecting, Events.Connected, States.Online);
            stateControl.AddTransition(States.Connecting, Events.CredsModified, States.Disconnecting);
            stateControl.AddTransition(States.Connecting, Events.LostConnection, States.Disconnecting);
            stateControl.AddTransition(States.Online, Events.LostConnection, States.Disconnecting);
            stateControl.AddTransition(States.Online, Events.CredsModified, States.Disconnecting);
            stateControl.AddTransition(States.Disconnecting, Events.DoneDisconnecting, States.Offline);
            stateControl.AddTransition(States.Disconnecting, ()=> testRequested, States.Connecting);

            stateControl.OnExit(States.Connecting, () =>
            {
                if (resultEvent != null)
                    resultEvent.SetResult(connectResult);
            });

            stateControl.OnEnter(States.Connecting, async () =>
            {
                var addressCopy = Address;
                var usernameCopy = Username;
                var pwdCopy = Password;

                connectCancellation = new CancellationTokenSource();

                try
                {
                    connectResult = await Connection.Connect(addressCopy, pwdCopy, addressCopy, connectCancellation.Token);
                    if (connectResult == ConnectionErrorCodes.None)
                        stateControl.PushEvent(Events.Connected);
                    else
                        stateControl.PushEvent(Events.ConnectFailed);
                }
                catch (Exception)
                {
                    // TO DO : log
                    connectResult = ConnectionErrorCodes.Unknown;
                    stateControl.PushEvent(Events.ConnectFailed);
                }
            });

            stateControl.OnEnter(States.Disconnecting, async () =>
            {
                try
                {
                    connectCancellation.Cancel();
                    await Connection.DisconnectAsync();
                }
                catch (Exception)
                {
                    // TO DO : log
                }
            });

        }

        private void Connection_StateChanged(ConnectionModel.States oldState, ConnectionModel.States newState)
        {
            if (newState == ConnectionModel.States.Offline)
                stateControl.PushEvent(Events.LostConnection);
        }

        public States State => stateControl.Current;
        public AccountModel Account { get; private set; }
        public ConnectionModel Connection { get; private set; }

        [DataMember]
        public string Address { get; private set; }
        [DataMember]
        public string Username { get; private set; }
        [DataMember]
        public string Password { get; private set; }

        public Task<ConnectionErrorCodes> TestConnection()
        {
            Task<ConnectionErrorCodes> toWait = null;

            stateControl.ModifyConditions(() =>
            {
                if (State == States.Online)
                    toWait = Task.FromResult(ConnectionErrorCodes.None);
                else
                {
                    resultEvent = new TaskCompletionSource<ConnectionErrorCodes>();
                    testRequested = true;
                    toWait = resultEvent.Task;
                }
            });

            return toWait;
        }

        public void Change(string address, string username, string password)
        {
            stateControl.ModifyConditions(() =>
            {
                if (connectCancellation != null)
                    connectCancellation.Cancel();

                stateControl.PushEvent(Events.CredsModified);
            });
        }
    }

    [DataContract]
    public class ClientModel2 : Actor
    {
        public enum States { Offline, Connecting, Online, Disconnecting }

        private CancellationTokenSource connectCancellation;
        private TaskCompletionSource<ConnectionErrorCodes> testRequest;

        private bool startRequested;
        private bool stopRequested;
        private bool lostConnection;

        public ClientModel2()
        {
            Connection = new ConnectionModel(null, new ConnectionOptions() { EnableFixLogs = false });
            Connection.State.StateChanged += (o, n) => Enqueue(() => { });
        }

        public States State { get; private set; }
        public AccountModel Account { get; private set; }
        public ConnectionModel Connection { get; private set; }

        [DataMember]
        public string Address { get; private set; }
        [DataMember]
        public string Username { get; private set; }
        [DataMember]
        public string Password { get; private set; }

        public Task<ConnectionErrorCodes> TestConnection()
        {
            return AsyncCall(() =>
            {
                if (State == States.Online)
                    return Task.FromResult(ConnectionErrorCodes.None);
                else
                {
                    if (testRequest == null)
                    {
                        ManageConnection();
                        testRequest = new TaskCompletionSource<ConnectionErrorCodes>();
                    }
                    return testRequest.Task;
                }
            });
        }

        private void ManageConnection()
        {
            if (State == States.Offline)
            {
                if (testRequest != null)
                    Connect();
            }
            else if (State == States.Online)
            {
                if (stopRequested)
                    Disconnect();
            }
        }

        private async void Disconnect()
        {
            State = States.Disconnecting;
            await Connection.DisconnectAsync();
            State = States.Offline;
            stopRequested = false;
            ManageConnection();
        }

        private async void Connect()
        {
            State = States.Connecting;
            var result = await Connection.Connect(Username, Password, Address, CancellationToken.None);
            if (result == ConnectionErrorCodes.None)
                State = States.Online;
            else
                State = States.Offline;
            testRequest?.TrySetResult(result);
            ManageConnection();
        }

        public void Change(string address, string username, string password)
        {
            Enqueue(() =>
            {
                Address = address;
                Username = username;
                Password = password;
                testRequest?.TrySetCanceled();
                stopRequested = true;
                ManageConnection();
            });
        }
    }

    [DataContract]
    public class ClientModel3
    {
        public enum States { Offline, Connecting, Online, Disconnecting }

        private object _sync = new object();
        private CancellationTokenSource connectCancellation;
        private TaskCompletionSource<ConnectionErrorCodes> testRequest;

        private bool stopRequested;
        private bool lostConnection;

        public ClientModel3()
        {
            Connection = new ConnectionModel(null, new ConnectionOptions() { EnableFixLogs = false });
            Connection.Disconnected += () => { lock (_sync) lostConnection = true; };
        }

        public void Init(object syncObj)
        {
            _sync = syncObj;
        }

        public States State { get; private set; }
        public ConnectionModel Connection { get; private set; }

        public event Action<ClientModel3> StateChanged;

        [DataMember]
        public string Address { get; private set; }
        [DataMember]
        public string Username { get; private set; }
        [DataMember]
        public string Password { get; private set; }

        public async Task<ConnectionErrorCodes> TestConnection()
        {
            Task<ConnectionErrorCodes> resultTask = null;

            lock (_sync)
            {
                if (State == States.Online)
                    return ConnectionErrorCodes.None;
                else
                {
                    if (testRequest == null)
                    {
                        ManageConnection();
                        testRequest = new TaskCompletionSource<ConnectionErrorCodes>();
                    }

                    resultTask = testRequest.Task;
                }
            }

            return await resultTask;
        }

        private void ManageConnection()
        {
            if (State == States.Offline)
            {
                if (testRequest != null)
                    Connect();
            }
            else if (State == States.Online)
            {
                if (stopRequested || lostConnection)
                    Disconnect();
            }
        }

        private async void Disconnect()
        {
            ChangeState(States.Disconnecting);
            await Connection.DisconnectAsync();

            lock (_sync)
            {
                ChangeState(States.Offline);
                stopRequested = false;
                lostConnection = false;
                ManageConnection();
            }
        }

        private void ChangeState(States newState)
        {
            State = newState;
            StateChanged?.Invoke(this);
        }

        private async void Connect()
        {
            ChangeState(States.Connecting);
            connectCancellation = new CancellationTokenSource();
            var result = await Connection.Connect(Username, Password, Address, connectCancellation.Token);
            if (result == ConnectionErrorCodes.None)
                ChangeState(States.Online);
            else
                ChangeState(States.Offline);
            testRequest?.TrySetResult(result);
            ManageConnection();
        }

        public void Change(string address, string username, string password)
        {
            lock (_sync)
            {
                Address = address;
                Username = username;
                Password = password;
                testRequest?.TrySetCanceled();
                stopRequested = true;
                connectCancellation?.Cancel();
                ManageConnection();
            }
        }
    }
}
