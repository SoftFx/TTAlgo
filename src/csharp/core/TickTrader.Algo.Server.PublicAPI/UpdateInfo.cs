using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace TickTrader.Algo.Server.PublicAPI
{
    public partial class UpdateInfo
    {
        public bool TryUnpack(out IUpdateInfo update)
        {
            update = null;

            var payload = Payload;

            if (payload.Is(AlgoServerMetadataUpdate.Descriptor))
                update = UpdateInfo<AlgoServerMetadataUpdate>.Unpack(this);

            else if (payload.Is(PackageUpdate.Descriptor))
                update = UpdateInfo<PackageUpdate>.Unpack(this);
            else if (payload.Is(AccountModelUpdate.Descriptor))
                update = UpdateInfo<AccountModelUpdate>.Unpack(this);
            else if (payload.Is(PluginModelUpdate.Descriptor))
                update = UpdateInfo<PluginModelUpdate>.Unpack(this);

            else if (payload.Is(PackageStateUpdate.Descriptor))
                update = UpdateInfo<PackageStateUpdate>.Unpack(this);
            else if (payload.Is(AccountStateUpdate.Descriptor))
                update = UpdateInfo<AccountStateUpdate>.Unpack(this);
            else if (payload.Is(PluginStateUpdate.Descriptor))
                update = UpdateInfo<PluginStateUpdate>.Unpack(this);

            else if (payload.Is(PluginStatusUpdate.Descriptor))
                update = UpdateInfo<PluginStatusUpdate>.Unpack(this);
            else if (payload.Is(PluginLogUpdate.Descriptor))
                update = UpdateInfo<PluginLogUpdate>.Unpack(this);
            else if (payload.Is(AlertListUpdate.Descriptor))
                update = UpdateInfo<AlertListUpdate>.Unpack(this);

            else if (payload.Is(HeartbeatUpdate.Descriptor))
                update = UpdateInfo<HeartbeatUpdate>.Unpack(this);

            return update != null;
        }
    }


    public interface IUpdateInfo
    {
        IMessage ValueMsg { get; }


        UpdateInfo Pack();
    }


    public class UpdateInfo<T> : IUpdateInfo
        where T : IMessage, new()
    {
        private Any _packedValue;


        public T Value { get; }

        public IMessage ValueMsg => Value;

        public Any PackedValue
        {
            get
            {
                if (_packedValue == null)
                    _packedValue = Any.Pack(Value);

                return _packedValue;
            }
        }


        public UpdateInfo(T value)
        {
            Value = value;
        }


        public static UpdateInfo<T> Unpack(UpdateInfo update)
        {
            var payload = update.Payload;
            var value = payload.Unpack<T>();
            return new UpdateInfo<T>(value) { _packedValue = payload };
        }


        public override string ToString()
        {
            return $"{{ ValueType = {Value.Descriptor.Name}, Value = {ValueMsg} }}";
        }

        public UpdateInfo Pack()
        {
            return new UpdateInfo { Payload = PackedValue };
        }
    }
}
