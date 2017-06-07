using System;
using System.IO;
using System.Linq;
using TickTrader.DedicatedServer.Extensions;

namespace TickTrader.DedicatedServer.DS.Models
{
    public interface IAlgoData
    {
        string Folder { get; }
        IFile[] Files { get; }

        void Clear();
        IFile GetFile(string decodedFile);
        void DeleteFile(string name);
    }

    public class AlgoData : IAlgoData
    {
        private object _syncObj;

        public AlgoData(string path, object syncObj)
        {
            Folder = path;
            _syncObj = syncObj;

            EnsureDirectoryCreated();
        }

        public string Folder { get; private set; }

        public IFile[] Files
        {
            get
            {
                if (Directory.Exists(Folder))
                {
                    var dInfo = new DirectoryInfo(Folder);
                    return dInfo.GetFiles().Select(fInfo => new ReadOnlyFileModel(fInfo.FullName)).ToArray();
                }
                else
                    return new ReadOnlyFileModel[0];
            }
        }

        public void Clear()
        {
            lock (_syncObj)
            {
                if (Directory.Exists(Folder))
                {
                    new DirectoryInfo(Folder).Clean();
                    Directory.Delete(Folder);
                }
            }
        }

        public void DeleteFile(string file)
        {
            File.Delete(Path.Combine(Folder, file));
        }

        public IFile GetFile(string file)
        {
            if (file.IsFileNameValid())
            {
                var fullPath = Path.Combine(Folder, file);

                return new ReadOnlyFileModel(fullPath);
            }

            throw new ArgumentException($"Incorrect file name {file}");
        }

        private void EnsureDirectoryCreated()
        {
            if (!Directory.Exists(Folder))
            {
                var dinfo = Directory.CreateDirectory(Folder);
            }
        }
    }
}
