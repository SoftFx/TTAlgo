using System;
using System.IO;
using System.Security.Cryptography;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Server.Persistence
{
    public static class CipherOptionsStorage
    {
        public static CipherV1Helper.ICipherOptions V1 { get; } = new CipherV1Options();


        private sealed class CipherV1Options : CipherV1Helper.ICipherOptions
        {
            private static readonly object _syncObj = new();
            private static readonly string _secretKeyDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ".ttalgo");
            private static readonly string _secretKeyPath = Path.Combine(_secretKeyDir, "cipher-v1.data");


            public void GetSecretKey(byte[] buffer)
            {
                lock (_syncObj)
                {
                    PathHelper.EnsureDirectoryCreated(_secretKeyDir);

                    if (File.Exists(_secretKeyPath))
                    {
                        using (var file = File.OpenRead(_secretKeyPath))
                        {
                            file.Read(buffer, 0, buffer.Length);
                        }
                    }
                    else
                    {
                        using (var rng = RandomNumberGenerator.Create())
                        {
                            rng.GetBytes(buffer);
                        }
                        File.WriteAllBytes(_secretKeyPath, buffer);
                    }
                }
            }
        }
    }
}
