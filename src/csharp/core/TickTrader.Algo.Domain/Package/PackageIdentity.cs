using Google.Protobuf.WellKnownTypes;
using System;
using System.IO;

namespace TickTrader.Algo.Domain
{
    public partial class PackageIdentity
    {
        public bool IsValid => string.IsNullOrEmpty(Hash);


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


        public static PackageIdentity Create(FileInfo info, string hash)
        {
            return new PackageIdentity(info.Name, info.FullName, info.CreationTimeUtc, info.LastWriteTimeUtc, info.Length, hash);
        }

        public static PackageIdentity CreateInvalid(FileInfo info)
        {
            return new PackageIdentity(info.Name, info.FullName, info.CreationTimeUtc, info.LastWriteTimeUtc, info.Length, "");
        }

        public static PackageIdentity CreateInvalid(string filePath)
        {
            return new PackageIdentity(Path.GetFileName(filePath), filePath, DateTime.UtcNow, DateTime.UtcNow, -1, "");
        }
    }
}
