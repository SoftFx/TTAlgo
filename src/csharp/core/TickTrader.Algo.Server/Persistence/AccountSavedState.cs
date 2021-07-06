using Google.Protobuf;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server.Persistence
{
    public class AccountSavedState
    {
        public string Id { get; set; }

        public string Server { get; set; }

        public string UserId { get; set; }

        public string DisplayName { get; set; }

        public string CredsUri { get; set; }

        public string CredsData { get; set; }


        public AccountSavedState Clone()
        {
            return new AccountSavedState
            {
                Id = Id,
                Server = Server,
                UserId = UserId,
                DisplayName = DisplayName,
                CredsUri = CredsUri,
                CredsData = CredsData,
            };
        }


        public AccountCreds UnpackCreds()
        {
            if (CredsUri == AccountCreds.Descriptor.FullName)
                return AccountCreds.Parser.ParseFrom(ByteString.CopyFromUtf8(CredsData));

            return null;
        }

        public void PackCreds(AccountCreds creds)
        {
            CredsUri = AccountCreds.Descriptor.FullName;
            CredsData = creds.ToByteString().ToStringUtf8();
        }
    }
}
