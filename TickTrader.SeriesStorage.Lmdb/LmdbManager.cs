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
            return new LmdbStorage(Path.Combine(_baseFolder, name + ".dat"), _readOnly);
        }

        public void DropStorage(string name)
        {
            throw new NotImplementedException();
        }
    }
}
