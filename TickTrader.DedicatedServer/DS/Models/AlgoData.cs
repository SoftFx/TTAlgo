using System;
using System.IO;
using System.Linq;
using TickTrader.DedicatedServer.Extensions;

namespace TickTrader.DedicatedServer.DS.Models
{
    public interface IAlgoData
    {
        string FullPath { get; }
        IFile[] Files { get; }

        void Clean();
        IFile GetFile(string decodedFile);
    }

    public class AlgoData : IAlgoData
    {
        private object _syncObj;

        public AlgoData(string path, object syncObj)
        {
            FullPath = path;
            _syncObj = syncObj;
        }

        public string FullPath { get; private set; }

        public IFile[] Files
        {
            get
            {
                if (Directory.Exists(FullPath))
                {
                    var dInfo = new DirectoryInfo(FullPath);
                    return dInfo.GetFiles().Select(fInfo => new ReadOnlyFileModel(fInfo.FullName)).ToArray();
                }
                else
                    return new ReadOnlyFileModel[0];
            }
        }

        public void Clean()
        {
            lock (_syncObj)
            {
                if (Directory.Exists(FullPath))
                {
                    new DirectoryInfo(FullPath).Clean();
                    Directory.Delete(FullPath);
                }
            }
        }

        public IFile GetFile(string file)
        {
            if (file.IsFileNameValid())
            {
                var fullPath = Path.Combine(FullPath, file);

                return new ReadOnlyFileModel(fullPath);
            }

            throw new ArgumentException($"Incorrect file name {file}");
        }
    }
}
