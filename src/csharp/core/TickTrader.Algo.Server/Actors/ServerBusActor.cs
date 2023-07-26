﻿using Google.Protobuf;
using System.Threading.Channels;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.Server
{
    internal class ServerBusActor : Actor
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<ServerBusActor>();

        private readonly ServerSnapshotBuilder _snapshotBuilder = new ServerSnapshotBuilder();
        private readonly ActorEventSource<IMessage> _updateEventSrc = new ActorEventSource<IMessage>();


        private ServerBusActor()
        {
            Receive<PackageUpdate>(OnPackageUpdate);
            Receive<PackageStateUpdate>(OnPackageStateUpdate);
            Receive<AccountModelUpdate>(OnAccountUpdate);
            Receive<AccountStateUpdate>(OnAccountStateUpdate);
            Receive<PluginModelUpdate>(OnPluginUpdate);
            Receive<PluginStateUpdate>(OnPluginStateUpdate);
            Receive<UpdateServiceStateUpdate>(OnUpdateServiceStatusChanged);

            Receive<PackageSnapshotRequest, PackageListSnapshot>(_ => _snapshotBuilder.GetPackageSnapshot());
            Receive<AccountSnapshotRequest, AccountListSnapshot>(_ => _snapshotBuilder.GetAccountSnapshot());
            Receive<PluginSnapshotRequest, PluginListSnapshot>(_ => _snapshotBuilder.GetPluginSnapshot());
            Receive<PluginInfoRequest, PluginModelInfo>(r => _snapshotBuilder.GetPluginInfo(r.PluginId));
            Receive<SubscribeToUpdatesRequest>(SubcribeToUpdates);
        }


        public static IActorRef Create()
        {
            return ActorSystem.SpawnLocal(() => new ServerBusActor(), $"{nameof(ServerBusActor)}");
        }


        private void OnPackageUpdate(PackageUpdate update)
        {
            if (_snapshotBuilder.UpdatePackage(update))
                _updateEventSrc.DispatchEvent(update);
        }

        private void OnPackageStateUpdate(PackageStateUpdate update)
        {
            if (_snapshotBuilder.UpdatePackageState(update))
                _updateEventSrc.DispatchEvent(update);
        }

        private void OnAccountUpdate(AccountModelUpdate update)
        {
            if (_snapshotBuilder.UpdateAccount(update))
                _updateEventSrc.DispatchEvent(update);
        }

        private void OnAccountStateUpdate(AccountStateUpdate update)
        {
            if (_snapshotBuilder.UpdateAccountState(update))
                _updateEventSrc.DispatchEvent(update);
        }

        private void OnPluginUpdate(PluginModelUpdate update)
        {
            if (_snapshotBuilder.UpdatePlugin(update))
                _updateEventSrc.DispatchEvent(update);
        }

        private void OnPluginStateUpdate(PluginStateUpdate update)
        {
            if (_snapshotBuilder.UpdatePluginState(update))
                _updateEventSrc.DispatchEvent(update);
        }

        private void OnUpdateServiceStatusChanged(UpdateServiceStateUpdate update)
        {
            if (_snapshotBuilder.OnUpdateServiceStateChanged(update))
                _updateEventSrc.DispatchEvent(update);
        }

        private void SubcribeToUpdates(SubscribeToUpdatesRequest request)
        {
            var sink = request.UpdateSink;
            if (sink != null)
            {
                if (request.SendSnapsnot)
                {
                    sink.TryWrite(_snapshotBuilder.GetPackageSnapshot());
                    sink.TryWrite(_snapshotBuilder.GetAccountSnapshot());
                    sink.TryWrite(_snapshotBuilder.GetPluginSnapshot());
                    sink.TryWrite(_snapshotBuilder.GetUpdateServiceSnapshot());
                }
                _updateEventSrc.Subscribe(sink);
            }
        }


        internal class PackageSnapshotRequest : Singleton<PackageSnapshotRequest> { }

        internal class AccountSnapshotRequest : Singleton<AccountSnapshotRequest> { }

        internal class PluginSnapshotRequest : Singleton<PluginSnapshotRequest> { }

        internal class PluginInfoRequest
        {
            public string PluginId { get; }

            public PluginInfoRequest(string pluginId)
            {
                PluginId = pluginId;
            }
        }

        internal class SubscribeToUpdatesRequest
        {
            public ChannelWriter<IMessage> UpdateSink { get; }

            public bool SendSnapsnot { get; }

            public SubscribeToUpdatesRequest(ChannelWriter<IMessage> updateSink, bool sendSnapsnot)
            {
                UpdateSink = updateSink;
                SendSnapsnot = sendSnapsnot;
            }
        }
    }
}
