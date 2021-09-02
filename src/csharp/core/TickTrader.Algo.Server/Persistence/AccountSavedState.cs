using Google.Protobuf;
using System;
using System.Security.Cryptography;
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
            {
                try
                {
                    var protectedBuffer = new byte[CredsData.Length / 2];
                    HexConverter.StringToBytes(CredsData, protectedBuffer);
                    var rawBuffer = ProtectedData.Unprotect(protectedBuffer, null, DataProtectionScope.CurrentUser);
                    return AccountCreds.Parser.ParseFrom(rawBuffer);
                }
                catch (SystemException ex) when (ex is PlatformNotSupportedException || ex is CryptographicException)
                {
                    return AccountCreds.Parser.ParseFrom(Convert.FromBase64String(CredsData));
                }
            }

            return null;
        }

        public void PackCreds(AccountCreds creds)
        {
            CredsUri = AccountCreds.Descriptor.FullName;

            var rawBuffer = creds.ToByteArray();
            try
            {
                var protectedBuffer = ProtectedData.Protect(rawBuffer, null, DataProtectionScope.CurrentUser);
                CredsData = HexConverter.BytesToString(protectedBuffer);
            }
            catch (PlatformNotSupportedException)
            {
                CredsData = Convert.ToBase64String(rawBuffer);
            }
        }
    }
}
