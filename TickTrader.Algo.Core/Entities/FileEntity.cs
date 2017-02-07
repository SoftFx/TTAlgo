using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class FileEntity : Api.File
    {
        public FileEntity(string path)
        {
            this.FullPath = path;
            this.IsNull = string.IsNullOrEmpty(path);
        }

        public string FullPath { get; private set; }
        public bool IsNull { get; private set; }

        public FileStream Open(FileMode mode)
        {
            return File.Open(FullPath, mode);
        }

        public byte[] ReadAllBytes()
        {
            return File.ReadAllBytes(FullPath);
        }

        public string ReadAllText()
        {
            return File.ReadAllText(FullPath);
        }
    }
}
