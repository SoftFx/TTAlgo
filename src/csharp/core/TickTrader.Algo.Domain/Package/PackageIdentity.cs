using Google.Protobuf.WellKnownTypes;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TickTrader.Algo.Domain
{
    public partial class PackageIdentity
    {
        public PackageIdentity(string fileName, string filePath, DateTime createdUtc, DateTime lastModifiedUtc, long size, string hash)
            : this(fileName, filePath, createdUtc.ToTimestamp(), lastModifiedUtc.ToTimestamp(), size, hash)
        {
        }

        public PackageIdentity(string fileName, string filePath, Timestamp createdUtc, Timestamp lastModifiedUtc, long size, string hash)
        {
            FileName = fileName;
            FilePath = filePath;
            CreatedUtc = createdUtc;
            LastModifiedUtc = lastModifiedUtc;
            Size = size;
            Hash = hash;
        }


        public static PackageIdentity Create(FileInfo info)
        {
            byte[] hashBytes;
            using (var file = info.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                hashBytes = SHA256.Create().ComputeHash(file);
            }
            var sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return new PackageIdentity(info.Name, info.FullName, info.CreationTimeUtc, info.LastWriteTimeUtc, info.Length, sb.ToString());
        }

        public static PackageIdentity CreateInvalid(FileInfo info)
        {
            return new PackageIdentity(info.Name, info.FullName, info.CreationTimeUtc, info.LastWriteTimeUtc, info.Length, "");
        }

        public static PackageIdentity CreateInvalid(string fileName, string filePath)
        {
            return new PackageIdentity(fileName, filePath, DateTime.UtcNow, DateTime.UtcNow, -1, "");
        }
    }
}
