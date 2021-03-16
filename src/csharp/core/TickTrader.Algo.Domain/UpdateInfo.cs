using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace TickTrader.Algo.Domain.ServerControl
{
    public partial class UpdateInfo
    {
        public bool TryUnpack(out IUpdateInfo update)
        {
            update = null;

            var payload = Payload;

            if (payload.Is(PackageUpdateInfo.Descriptor))
                update = UpdateInfo<PackageUpdateInfo>.Unpack(this);
            else if (payload.Is(PackageStateUpdateInfo.Descriptor))
                update = UpdateInfo<PackageStateUpdateInfo>.Unpack(this);
            else if (payload.Is(AccountUpdateInfo.Descriptor))
                update = UpdateInfo<AccountUpdateInfo>.Unpack(this);
            else if (payload.Is(AccountStateUpdateInfo.Descriptor))
                update = UpdateInfo<AccountStateUpdateInfo>.Unpack(this);
            else if (payload.Is(PluginUpdateInfo.Descriptor))
                update = UpdateInfo<PluginUpdateInfo>.Unpack(this);
            else if (payload.Is(PluginStateUpdateInfo.Descriptor))
                update = UpdateInfo<PluginStateUpdateInfo>.Unpack(this);

            return update != null;
        }
    }


    public interface IUpdateInfo
    {
        UpdateInfo.Types.UpdateType Type { get; }

        IMessage ValueMsg { get; }


        UpdateInfo Pack();
    }


    public class UpdateInfo<T> : IUpdateInfo
        where T : IMessage, new()
    {
        private Any _packedValue;


        public UpdateInfo.Types.UpdateType Type { get; }

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


        public UpdateInfo(UpdateInfo.Types.UpdateType type, T value)
        {
            Type = type;
            Value = value;
        }


        public static UpdateInfo<T> Unpack(UpdateInfo update)
        {
            var payload = update.Payload;
            var value = payload.Unpack<T>();
            return new UpdateInfo<T>(update.Type, value) { _packedValue = payload };
        }


        public override string ToString()
        {
            return $"{{ Type = {Type}, ValueType = {Value.Descriptor.Name}, Value = {ValueMsg} }}";
        }

        public UpdateInfo Pack()
        {
            return new UpdateInfo { Type = Type, Payload = PackedValue };
        }
    }
}
