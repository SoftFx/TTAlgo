using System;

namespace TickTrader.Algo.Domain
{
    public class AccountId : IEquatable<AccountId>, IComparable<AccountId>
    {
        public string PackedStr { get; }

        public string Server { get; }

        public string UserId { get; }


        public AccountId(string server, string userId)
            : this(Pack(server, userId), server, userId)
        {
        }

        private AccountId(string packedStr, string server, string userId)
        {
            PackedStr = packedStr;
            Server = server;
            UserId = userId;
        }


        public static string Pack(string server, string userId)
        {
            if (string.IsNullOrWhiteSpace(server))
                throw new ArgumentException($"'{nameof(server)}' can't be empty string");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException($"'{nameof(userId)}' can't be empty string");

            return string.Join(SharedConstants.IdSeparatorStr, SharedConstants.AccountIdPrefix, server, userId);
        }

        public static bool TryUnpack(string accountId, out AccountId accId)
        {
            accId = new AccountId(accountId, SharedConstants.InvalidIdPart, SharedConstants.InvalidIdPart);

            var parts = accountId.Split(SharedConstants.IdSeparator);

            if (parts.Length != 3 || parts[0] != SharedConstants.AccountIdPrefix)
                return false;

            accId = new AccountId(accountId, parts[1], parts[2]);

            return true;
        }

        public static void Unpack(string accountId, out AccountId accId)
        {
            if (!TryUnpack(accountId, out accId))
                throw new ArgumentException("Invalid account id");
        }

        public static void Unpack(string accountId, out string login, out string server)
        {
            if (!TryUnpack(accountId, out var accId))
                throw new ArgumentException("Invalid account id");

            login = accId?.UserId;
            server = accId?.Server;
        }

        public override int GetHashCode()
        {
            return PackedStr.GetHashCode();
        }

        public override bool Equals(object other)
        {
            return Equals(other as AccountId);
        }

        public bool Equals(AccountId other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return PackedStr == other.PackedStr;
        }

        public int CompareTo(AccountId other)
        {
            if (other == null)
                return 1;

            if (ReferenceEquals(this, other))
                return 0;

            return StringComparer.Ordinal.Compare(PackedStr, other.PackedStr);
        }
    }
}
