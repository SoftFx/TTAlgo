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

        public LmdbManager(string baseFolder)
        {
            _baseFolder = baseFolder;
        }

        public IEnumerable<string> GetStorages()
        {
            return Directory.GetFiles(_baseFolder);
        }

        public IKeyValueBinaryStorage OpenStorage(string name)
        {
            return new LmdbStorage(Path.Combine(_baseFolder, name));
        }
    }
}
