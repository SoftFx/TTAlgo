using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TickTrader.BotAgent.BA.Entities
{
    public class AccountInfo
    {
        public AccountInfo(string server, string login, bool useNewProtocol)
        {
            Server = server;
            Login = login;
            UseSfxProtocol = useNewProtocol;
        }

        public string Server { get; set; }
        public string Login { get; set; }
        public bool UseSfxProtocol { get; set; }
    }
}
