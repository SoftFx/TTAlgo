using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    internal class ServerAuthEntry
    {
        public ServerAuthEntry(ServerElement cfgElement)
            : this(cfgElement.Name, cfgElement.ShortName, cfgElement.Address, cfgElement.Color)
        {
        }

        public ServerAuthEntry(string address)
            : this(address, address, address, null)
        {
        }

        public ServerAuthEntry(string name, string shortName, string address, string color)
        {
            this.Name = name;
            this.Address = address;
            this.ShortName = shortName;

            if (string.IsNullOrWhiteSpace(this.ShortName))
                this.ShortName = name;

            if (!string.IsNullOrWhiteSpace(color))
                this.Color = (Color)ColorConverter.ConvertFromString(color);

            if (string.IsNullOrWhiteSpace(name))
                this.Name = address;
        }

        public string Name { get; }
        public string ShortName { get; }
        public string Address { get; }
        public Color Color { get; }
    }

    internal class AccountAuthEntry
    {
        private AccountSorageEntry storageRecord;

        public AccountAuthEntry(AccountSorageEntry storageRecord, ServerAuthEntry server)
        {
            this.storageRecord = storageRecord;
            this.Server = server;
        }

        public ServerAuthEntry Server { get; private set; }
        public string Login { get { return storageRecord.Login; } }
        public bool HasPassword { get { return storageRecord.HasPassword; } }

        public bool Matches(AccountSorageEntry acc)
        {
            return Login == acc.Login && Server.Address == acc.ServerAddress;
        }

        public string Password
        {
            get { return storageRecord.Password; }
            set { storageRecord.Password = value; }
        }

        public override bool Equals(object obj)
        {
            var entry = obj as AccountAuthEntry;
            return entry != null && entry.Login == Login && entry.Server.Address == Server.Address;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)2166136261;
                // Suitable nullity checks etc, of course :)
                hash = (hash * 16777619) ^ Login.GetHashCode();
                hash = (hash * 16777619) ^ Server.Address.GetHashCode();
                return hash;
            }
        }
    }
}
