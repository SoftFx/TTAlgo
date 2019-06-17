using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.BotAgent.Common;

namespace TickTrader.BotAgent.Configurator
{
    static class CryptoManager
    {
        private static KeyGenerator _generator = new KeyGenerator();

        public static string GetNewPassword(int lenght)
        {
            return _generator.GetUniqueKey(lenght);
        }
    }
}
