using ProtoBuf;
using System;

namespace TickTrader.FeedStorage
{
    [ProtoContract]
    public class CustomCurrency
    {
        [ProtoIgnore]
        internal Guid StorageId { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }
        [ProtoMember(3)]
        public int Digits { get; set; }
    }
}
