using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TickTrader.SeriesStorage.Lmdb
{
    public class LmdbManager : IBinaryStorageManager
    {
        private string _baseFolder;
        private bool _readOnly;

        public LmdbManager(string baseFolder, bool readOnly = false)
        {
            _baseFolder = baseFolder;
            _readOnly = readOnly;

            Directory.CreateDirectory(baseFolder);
        }

        public bool SupportsStorageDrop => true;
        public GetStorageSizeMode GetSizeMode => GetStorageSizeMode.ByManager;

        public IEnumerable<string> GetStorages()
        {
            try
            {
                return Directory.GetFiles(_baseFolder, "*.dat")
                    .Where(f => Path.GetExtension(f).ToLower() == ".dat")
                    .Select(f => DecodeName(Path.GetFileNameWithoutExtension(f)))
                    .ToList();
            }
            catch (DirectoryNotFoundException) { }

            return new List<string>();
        }

        public IKeyValueBinaryStorage OpenStorage(string name)
        {
            return new LmdbStorage(GetDataFileName(name), _readOnly);
        }

        public void DropStorage(string name)
        {
            File.Delete(GetDataFileName(name));
            File.Delete(GetLockFileName(name));
        }

        public long GetStorageSize(string name)
        {
            var info = new FileInfo(GetDataFileName(name));
            return info.Length;
        }

        private string GetDataFileName(string dbName)
        {
            var safeFileName = EncodeName(dbName);
            return Path.Combine(_baseFolder, safeFileName + ".dat");
        }

        private string GetLockFileName(string dbName)
        {
            var safeFileName = EncodeName(dbName);
            return Path.Combine(_baseFolder, safeFileName + ".dat-lock");
        }

        private string EncodeName(string dbName)
        {
            return Uri.EscapeDataString(dbName);
        }

        private string DecodeName(string safeFileName)
        {
            return Uri.UnescapeDataString(safeFileName);
        }
    }
}
