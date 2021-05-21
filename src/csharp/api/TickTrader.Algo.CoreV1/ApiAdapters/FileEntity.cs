using System.IO;

namespace TickTrader.Algo.CoreV1
{
    public class FileEntity : Api.File
    {
        public FileEntity(string path)
        {
            FullPath = path;
            IsNull = string.IsNullOrEmpty(path);
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
