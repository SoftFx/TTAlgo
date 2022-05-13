using System;
using System.Buffers;
using System.IO;
using System.Security.Cryptography;

namespace TickTrader.Algo.Core.Lib
{
    public class CipherV1Helper
    {
        public interface ICipherOptions
        {
            void GetSecretKey(byte[] buffer);
        }


        public const int SecretKeySize = 4096;
        private const int BlockSize = 16;
        private const int CipherKeySize = 32;
        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 16384;


        public static string Encrypt(ICipherOptions options, ArraySegment<byte> plainData)
        {
            // Salt and IV is randomly generated each time, but is prepended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.
            var secretKey = ArrayPool<byte>.Shared.Rent(SecretKeySize);
            var saltData = ArrayPool<byte>.Shared.Rent(CipherKeySize);

            // https://datatracker.ietf.org/doc/html/rfc5652#section-6.3
            // Padding adds extra block if data length is multiple of block size
            var dataBlockCnt = plainData.Count / BlockSize + 1;
            var cipherLength = CipherKeySize + BlockSize * dataBlockCnt;
            var cipherData = ArrayPool<byte>.Shared.Rent(cipherLength);

            try
            {
                options.GetSecretKey(secretKey);

                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetNonZeroBytes(saltData);
                }

                using (var deriveBytes = new Rfc2898DeriveBytes(secretKey, saltData, DerivationIterations))
                {
                    var aesKeyData = deriveBytes.GetBytes(CipherKeySize);
                    var ivData = deriveBytes.GetBytes(BlockSize);
                    using (var aes = Aes.Create())
                    {
                        aes.BlockSize = 8 * BlockSize;
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.PKCS7;
                        using (var encryptor = aes.CreateEncryptor(aesKeyData, ivData))
                        using (var memoryStream = new MemoryStream(cipherData, 0, cipherLength))
                        {
                            memoryStream.Write(saltData, 0, CipherKeySize);
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainData.Array, 0, plainData.Count);
                                cryptoStream.FlushFinalBlock();
                            }
                        }
                    }
                }

                return HexConverter.BytesToString(cipherData.AsSpan(0, cipherLength));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(secretKey);
                ArrayPool<byte>.Shared.Return(saltData);
                ArrayPool<byte>.Shared.Return(cipherData);
            }
        }

        public static ReadOnlySequence<byte> Decrypt(ICipherOptions options, string cipherText)
        {
            // Get the complete stream of bytes that represent:
            // [KeysizeBytes bytes of Salt] + [KeysizeBytes bytes of IV] + [n bytes of CipherText]

            var secretKey = ArrayPool<byte>.Shared.Rent(SecretKeySize);
            var saltData = ArrayPool<byte>.Shared.Rent(CipherKeySize);

            var cipherDataLength = cipherText.Length / 2;
            var cipherData = ArrayPool<byte>.Shared.Rent(cipherDataLength);
            var cryptoDataLength = cipherDataLength - CipherKeySize;
            var plainDataBuffer = new byte[cryptoDataLength];

            try
            {
                options.GetSecretKey(secretKey);

                HexConverter.StringToBytes(cipherText, cipherData.AsSpan(0, cipherDataLength));

                cipherData.AsSpan(0, CipherKeySize).CopyTo(saltData);

                using (var memoryStream = new MemoryStream(cipherData, CipherKeySize, cryptoDataLength))
                using (var deriveBytes = new Rfc2898DeriveBytes(secretKey, saltData, DerivationIterations))
                {
                    var aesKeyData = deriveBytes.GetBytes(CipherKeySize);
                    var ivData = deriveBytes.GetBytes(BlockSize);
                    using (var aes = Aes.Create())
                    {
                        aes.BlockSize = 8 * BlockSize;
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.PKCS7;
                        using (var decryptor = aes.CreateDecryptor(aesKeyData, ivData))
                        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            var plainDataLength = 0;
                            var read = 0;
                            do
                            {
                                read = cryptoStream.Read(plainDataBuffer, plainDataLength, plainDataBuffer.Length - plainDataLength);
                                plainDataLength += read;
                            }
                            while (read > 0);
                            return new ReadOnlySequence<byte>(plainDataBuffer, 0, plainDataLength);
                        }
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(secretKey);
                ArrayPool<byte>.Shared.Return(saltData);
                ArrayPool<byte>.Shared.Return(cipherData);
            }
        }
    }
}
