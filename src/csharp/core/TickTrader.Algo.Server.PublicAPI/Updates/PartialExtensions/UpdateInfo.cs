using Google.Protobuf;
using Google.Protobuf.Reflection;
using System.Collections.Generic;

namespace TickTrader.Algo.Server.PublicAPI
{
    public partial class UpdateInfo
    {
        private static readonly Dictionary<Types.PayloadType, MessageDescriptor> _updateDescriptorMap = new Dictionary<Types.PayloadType, MessageDescriptor>();
        private static readonly Dictionary<string, Types.PayloadType> _updateTypeMap = new Dictionary<string, Types.PayloadType>();


        public bool TryUnpack(out IUpdateInfo update)
        {
            InitDescriptorCache();

            update = null;

            var type = Type;

            if (!_updateDescriptorMap.TryGetValue(type, out var decriptor))
                return false;

            var msg = decriptor.Parser.ParseFrom(Payload);
            switch (type)
            {
                case Types.PayloadType.Heartbeat: update = UpdateInfo<HeartbeatUpdate>.Unpack(msg); break;

                case Types.PayloadType.ServerMetadataUpdate: update = UpdateInfo<AlgoServerMetadataUpdate>.Unpack(msg); break;

                case Types.PayloadType.PackageUpdate: update = UpdateInfo<PackageUpdate>.Unpack(msg); break;
                case Types.PayloadType.PackageStateUpdate: update = UpdateInfo<PackageStateUpdate>.Unpack(msg); break;

                case Types.PayloadType.AccountModelUpdate: update = UpdateInfo<AccountModelUpdate>.Unpack(msg); break;
                case Types.PayloadType.AccountStateUpdate: update = UpdateInfo<AccountStateUpdate>.Unpack(msg); break;

                case Types.PayloadType.PluginModelUpdate: update = UpdateInfo<PluginModelUpdate>.Unpack(msg); break;
                case Types.PayloadType.PluginStateUpdate: update = UpdateInfo<PluginStateUpdate>.Unpack(msg); break;
                case Types.PayloadType.PluginLogUpdate: update = UpdateInfo<PluginLogUpdate>.Unpack(msg); break;
                case Types.PayloadType.PluginStatusUpdate: update = UpdateInfo<PluginStatusUpdate>.Unpack(msg); break;

                case Types.PayloadType.AlertListUpdate: update = UpdateInfo<AlertListUpdate>.Unpack(msg); break;
            }

            return update != null;
        }

        public static bool TryPack(IMessage msg, out UpdateInfo update)
        {
            InitDescriptorCache();

            if (msg is UpdateInfo updateInfo)
            {
                update = updateInfo;
                return true;
            }

            update = null;
            if (!_updateTypeMap.TryGetValue(msg.Descriptor.FullName, out var type))
                return false;

            update = new UpdateInfo { Type = type, Payload = msg.ToByteString(), };
            return true;
        }


        private static void InitDescriptorCache()
        {
            // protobuf code gen uses static ctor to init Descriptor property

            if (_updateDescriptorMap.Count > 0)
                return;

            RegisterDescriptor(HeartbeatUpdate.Descriptor, Types.PayloadType.Heartbeat);
            RegisterDescriptor(AlgoServerMetadataUpdate.Descriptor, Types.PayloadType.ServerMetadataUpdate);
            RegisterDescriptor(PackageUpdate.Descriptor, Types.PayloadType.PackageUpdate);
            RegisterDescriptor(PackageStateUpdate.Descriptor, Types.PayloadType.PackageStateUpdate);
            RegisterDescriptor(AccountModelUpdate.Descriptor, Types.PayloadType.AccountModelUpdate);
            RegisterDescriptor(AccountStateUpdate.Descriptor, Types.PayloadType.AccountStateUpdate);
            RegisterDescriptor(PluginModelUpdate.Descriptor, Types.PayloadType.PluginModelUpdate);
            RegisterDescriptor(PluginStateUpdate.Descriptor, Types.PayloadType.PluginStateUpdate);
            RegisterDescriptor(PluginLogUpdate.Descriptor, Types.PayloadType.PluginLogUpdate);
            RegisterDescriptor(PluginStatusUpdate.Descriptor, Types.PayloadType.PluginStatusUpdate);
        }

        private static void RegisterDescriptor(MessageDescriptor descriptor, Types.PayloadType type)
        {
            _updateDescriptorMap[type] = descriptor;
            _updateTypeMap[descriptor.FullName] = type;
        }
    }


    public interface IUpdateInfo
    {
        IMessage ValueMsg { get; }
    }


    public class UpdateInfo<T> : IUpdateInfo
        where T : IMessage, new()
    {
        public T Value { get; }

        public IMessage ValueMsg => Value;


        public UpdateInfo(T value)
        {
            Value = value;
        }


        public static UpdateInfo<T> Unpack(IMessage msg)
        {
            return new UpdateInfo<T>((T)msg);
        }
    }
}
