using System.IO;
using System.Security.Cryptography;

namespace TickTrader.Algo.Core.Lib
{
    public class FileHelper
    {
        public static string CalculateSha256Hash(FileInfo info)
        {
            byte[] hashBytes;
            using (var file = info.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var hashAlgo = SHA256.Create())
            {
                hashBytes = hashAlgo.ComputeHash(file);
            }
            return HexConverter.BytesToString(hashBytes);
        }

        public static string CalculateSha256Hash(FileStream stream)
        {
            byte[] hashBytes;
            using (var hashAlgo = SHA256.Create())
            {
                hashBytes = SHA256.Create().ComputeHash(stream);
            }
            return HexConverter.BytesToString(hashBytes);
        }
    }
}
