using System;

namespace TickTrader.Algo.Domain
{
    public static class AccountId
    {
        public static string Pack(string server, string userId)
        {
            if (string.IsNullOrWhiteSpace(server))
                throw new ArgumentException($"'{nameof(server)}' can't be empty string");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException($"'{nameof(userId)}' can't be empty string");

            return string.Join(SharedConstants.IdSeparatorStr, SharedConstants.AccountIdPrefix, server, userId);
        }

        public static bool TryUnpack(string accountId, out string server, out string userId)
        {
            server = SharedConstants.InvalidIdPart;
            userId = SharedConstants.InvalidIdPart;

            var parts = accountId.Split(SharedConstants.IdSeparator);

            if (parts.Length != 3 || parts[0] != SharedConstants.AccountIdPrefix)
                return false;

            server = parts[1];
            userId = parts[2];

            return true;
        }

        public static void Unpack(string accountId, out string server, out string userId)
        {
            if (!TryUnpack(accountId, out server, out userId))
                throw new ArgumentException("Invalid account id");
        }
    }
}
