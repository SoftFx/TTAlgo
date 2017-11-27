using System;
using System.IO;

namespace TickTrader.BotAgent.BA.Models
{
    public interface IFile
    {
        long Size { get; }
        string Name { get; }
        string Extension { get; }
        byte[] ReadAllBytes();
        Stream OpenRead();
        Stream Open(FileMode mode);
        bool Exists { get; }
    }

    public class ReadOnlyFileModel : IFile
    {
        private string _fullName;

        public ReadOnlyFileModel(string fullName)
        {
            _fullName = fullName;

            if (Exists)
            {
                var fi = new FileInfo(_fullName);
                Name = fi.Name;
                Size = fi.Length;
                Extension = fi.Extension;
            }
            else
            {
                Name = Path.GetFileName(fullName);
                Extension = Path.GetExtension(Name);
                Size = 0;
            }
        }

        public long Size { get; private set; }

        public string Name { get; private set; }

        public string Extension { get; private set; }

        public bool Exists => File.Exists(_fullName);

        public byte[] ReadAllBytes()
        {
            throw new NotImplementedException();
        }

        Stream IFile.Open(FileMode mode)
        {
            throw new NotSupportedException();
        }

        public Stream OpenRead()
        {
            return File.OpenRead(_fullName);
        }
    }
}
