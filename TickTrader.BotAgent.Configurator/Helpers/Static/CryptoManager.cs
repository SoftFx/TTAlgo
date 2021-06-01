using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotAgent.Configurator
{
    internal static class CryptoManager
    {
        private static readonly KeyGenerator _generator = new KeyGenerator();


        public static string GetNewPassword(int length) => _generator.GetUniqueKey(length);
    }
}
