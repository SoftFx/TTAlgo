using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    .Select(f => Path.GetFileNameWithoutExtension(f))
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
            return Path.Combine(_baseFolder, dbName + ".dat");
        }

        private string GetLockFileName(string dbName)
        {
            return Path.Combine(_baseFolder, dbName + ".dat-lock");
        }
    }
}
