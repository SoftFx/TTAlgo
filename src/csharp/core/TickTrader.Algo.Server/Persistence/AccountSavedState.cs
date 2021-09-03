using Google.Protobuf;
using System;
using System.Buffers;
using TickTrader.Algo.Core.Lib;
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

        public string CredsVersion { get; set; }

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
                CredsVersion = CredsVersion,
                CredsData = CredsData,
            };
        }


        public AccountCreds UnpackCreds()
        {
            var version = string.IsNullOrEmpty(CredsVersion) ? "v0" : CredsVersion;

            switch (version)
            {
                case "v0": return AccountCreds.Parser.ParseFrom(Convert.FromBase64String(CredsData));
                case "v1":
                    var plainData = CipherV1Helper.Decrypt(CipherOptionsStorage.V1, CredsData);
                    return AccountCreds.Parser.ParseFrom(plainData);
            }

            return null;
        }

        public void PackCreds(AccountCreds creds)
        {
            CredsUri = AccountCreds.Descriptor.FullName;

            var size = creds.CalculateSize();
            var buffer = ArrayPool<byte>.Shared.Rent(size);
            try
            {
                var plainData = new ArraySegment<byte>(buffer, 0, size);
                creds.WriteTo(plainData);
                CredsData = CipherV1Helper.Encrypt(CipherOptionsStorage.V1, plainData);
                CredsVersion = "v1";
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
