using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotAgent.BA.Entities
{
    public class PluginInfo
    {
        public PluginInfo(PluginKey key, AlgoPluginDescriptor descriptor)
        {
            Id = key;
            Descriptor = descriptor;
        }

        public PluginKey Id { get; }
        public AlgoPluginDescriptor Descriptor { get; }
    }

    public class PluginKey
    {
        public PluginKey(string package, string id)
        {
            PackageName = package;
            DescriptorId = id;
        }

        public string PackageName { get; private set; }
        public string DescriptorId { get; private set; }
    }

    public class AccountKey
    {
        public AccountKey(string login, string server)
        {
            Login = login;
            Server = server;
        }

        public string Login { get; private set; }
        public string Server { get; private set; }
    }
}
