using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography;

namespace TickTrader.Algo.Core.Lib.Tests
{
    [TestClass]
    public class CipherV1HelperTests
    {
        [TestMethod]
        public void CheckCommonPayloadSizes()
        {
            var data = new byte[64];

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetNonZeroBytes(data);
            }

            for (var size = 1; size <= data.Length; size++)
            {
                var cipherText = CipherV1Helper.Encrypt(MockCipherOptions.Instance, new ArraySegment<byte>(data, 0, size));
                var plainData = CipherV1Helper.Decrypt(MockCipherOptions.Instance, cipherText);
                for (var j = 0; j < size; j++)
                {
                    if (data[j] != plainData.FirstSpan[j])
                        throw new Exception($"Failed to decypher payload with size={size} at position {j}");
                }
            }
        }


        private class MockCipherOptions : Singleton<MockCipherOptions>, CipherV1Helper.ICipherOptions
        {
            private static readonly byte[] _key;

            static MockCipherOptions()
            {
                _key = new byte[CipherV1Helper.SecretKeySize];
                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetNonZeroBytes(_key);
                }
            }

            public void GetSecretKey(byte[] buffer)
            {
                _key.CopyTo(buffer.AsSpan());
            }
        }
    }
}
