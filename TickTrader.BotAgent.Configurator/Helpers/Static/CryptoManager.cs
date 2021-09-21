using OtpNet;
using System.Security.Cryptography;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotAgent.Configurator
{
    internal static class CryptoManager
    {
        private static readonly KeyGenerator _generator = new KeyGenerator();


        public static string GetNewPassword(int length) => _generator.GetUniqueKey(length);

        public static string GenerateOtpSecret(int length)
        {
            var data = new byte[length];

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetNonZeroBytes(data);
            }
            return Base32Encoding.ToString(data).ToLowerInvariant();
        }
    }
}
