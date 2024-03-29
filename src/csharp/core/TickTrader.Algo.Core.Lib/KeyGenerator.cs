﻿using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TickTrader.Algo.Core.Lib
{
    public sealed class KeyGenerator
    {
        private readonly string _availableCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";


        public KeyGenerator(string availableCharacters = null)
        {
            _availableCharacters = availableCharacters ?? _availableCharacters;
        }

        public string GetUniqueKey(int length)
        {
            byte[] data;

            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                data = new byte[length];
                crypto.GetNonZeroBytes(data);
            }

            StringBuilder result = new StringBuilder(length);

            foreach (byte b in data)
            {
                result.Append(_availableCharacters[b % _availableCharacters.Length]);
            }

            return result.ToString();
        }

        public string GetRandomString(int length)
        {
            var random = new Random();

            return new string(Enumerable.Repeat(_availableCharacters, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
