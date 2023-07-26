﻿using Google.Protobuf;
using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    public class ServerBusModel
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<ServerBusModel>();

        private readonly ChannelEventSource<IMessage> _updateEventSrc = new ChannelEventSource<IMessage>();
        private readonly ChannelEventSource<PackageUpdate> _pkgUpdateEventSrc = new ChannelEventSource<PackageUpdate>();
        private readonly ChannelEventSource<PackageStateUpdate> _pkgStateEventSrc = new ChannelEventSource<PackageStateUpdate>();
        private readonly ChannelEventSource<AccountModelUpdate> _accUpdateEventSrc = new ChannelEventSource<AccountModelUpdate>();
        private readonly ChannelEventSource<AccountStateUpdate> _accStateEventSrc = new ChannelEventSource<AccountStateUpdate>();
        private readonly ChannelEventSource<PluginModelUpdate> _pluginUpdateEventSrc = new ChannelEventSource<PluginModelUpdate>();
        private readonly ChannelEventSource<PluginStateUpdate> _pluginStateEventSrc = new ChannelEventSource<PluginStateUpdate>();

        private IActorRef _ref;


        public IEventSource<PackageUpdate> PackageUpdated => _pkgUpdateEventSrc;

        public IEventSource<PackageStateUpdate> PackageStateUpdated => _pkgStateEventSrc;

        public IEventSource<AccountModelUpdate> AccountUpdated => _accUpdateEventSrc;

        public IEventSource<AccountStateUpdate> AccountStateUpdated => _accStateEventSrc;

        public IEventSource<PluginModelUpdate> PluginUpdated => _pluginUpdateEventSrc;

        public IEventSource<PluginStateUpdate> PluginStateUpdated => _pluginStateEventSrc;


        public ServerBusModel() { }


        public void Dispose()
        {
            _updateEventSrc.Dispose();
            _pkgUpdateEventSrc.Dispose();
            _pkgStateEventSrc.Dispose();
            _accUpdateEventSrc.Dispose();
            _accStateEventSrc.Dispose();
            _pluginUpdateEventSrc.Dispose();
            _pluginStateEventSrc.Dispose();
        }


        public Task<PackageListSnapshot> GetPackageSnapshot() => _ref.Ask<PackageListSnapshot>(ServerBusActor.PackageSnapshotRequest.Instance);

        public Task<AccountListSnapshot> GetAccountSnapshot() => _ref.Ask<AccountListSnapshot>(ServerBusActor.AccountSnapshotRequest.Instance);

        public Task<PluginListSnapshot> GetPluginSnapshot() => _ref.Ask<PluginListSnapshot>(ServerBusActor.PluginSnapshotRequest.Instance);

        public Task<PluginModelInfo> GetPluginInfo(string pluginId) => _ref.Ask<PluginModelInfo>(new ServerBusActor.PluginInfoRequest(pluginId));

        public Task SubscribeToUpdates(ChannelWriter<IMessage> updateSink, bool sendSnapshot) => _ref.Ask(new ServerBusActor.SubscribeToUpdatesRequest(updateSink, sendSnapshot));


        internal void Init(IActorRef actor)
        {
            _ref = actor;
            _ref.Tell(new ServerBusActor.SubscribeToUpdatesRequest(_updateEventSrc.Writer, false));

            _updateEventSrc.Subscribe(DispatchUpdate);
        }


        private void DispatchUpdate(IMessage update)
        {
            switch (update)
            {
                case PackageUpdate pkgUpdate: _pkgUpdateEventSrc.Send(pkgUpdate); break;
                case PackageStateUpdate pkgStateUpdate: _pkgStateEventSrc.Send(pkgStateUpdate); break;
                case AccountModelUpdate accUpdate: _accUpdateEventSrc.Send(accUpdate); break;
                case AccountStateUpdate accStateUpdate: _accStateEventSrc.Send(accStateUpdate); break;
                case PluginModelUpdate pluginUpdate: _pluginUpdateEventSrc.Send(pluginUpdate); break;
                case PluginStateUpdate pluginStateUpdate: _pluginStateEventSrc.Send(pluginStateUpdate); break;
                case Domain.ServerControl.UpdateServiceStateUpdate updateSvcStateUpdate: break; // not required there for now

                default: _logger.Error($"Unexpected update of type '{update.Descriptor.FullName}'"); break;
            }
        }
    }
}
