using ActorSharp;
using NLog;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.BotAgent.Extensions;

namespace TickTrader.BotAgent.BA.Models
{
    public class AlgoData : Actor
    {
        private static ILogger _log = LogManager.GetLogger(nameof(AlgoData));

        private string _folder;

        private void Init(string path)
        {
            _folder = path;

            EnsureDirectoryCreated();
        }

        public class ControlHandler : Handler<AlgoData>
        {
            public ControlHandler(string botId)
                : base(SpawnLocal<AlgoData>(null, "AlgoData: " + botId))
            {
                Actor.Send(a => a.Init(ServerModel.GetWorkingFolderFor(botId)));
            }

            public Ref<AlgoData> Ref => Actor;
            public Task<string> GetFolder() => Actor.Call(a => a._folder);
            public Task EnsureDirectoryCreated() => Actor.Call(a => a.EnsureDirectoryCreated());
            public Task Clear() => Actor.Call(a => a.Clear());
        }

        public class Handler : BlockingHandler<AlgoData>, IBotFolder
        {
            public Handler(Ref<AlgoData> logRef) : base(logRef) { }

            public Task<string> GetFolder() => CallActorAsync(a => a._folder);
            public Task<IFile[]> GetFiles() => CallActorAsync(a => a.GetFiles());
            public Task Clear() => CallActorAsync(a => a.Clear());
            public Task DeleteFile(string file) => CallActorAsync(a => a.DeleteFile(file));
            public Task<IFile> GetFile(string file) => CallActorAsync(a => a.GetFile(file));
            public Task SaveFile(string file, byte[] bytes) => CallActorAsync(a => a.SaveFile(file, bytes));
            public Task<string> GetFileReadPath(string file) => CallActorAsync(a => a.GetFileReadPath(file));
            public Task<string> GetFileWritePath(string file) => CallActorAsync(a => a.GetFileWritePath(file));
        }

        private IFile[] GetFiles()
        {
            if (Directory.Exists(_folder))
            {
                var dInfo = new DirectoryInfo(_folder);
                return dInfo.GetFiles().Select(fInfo => new ReadOnlyFileModel(fInfo.FullName)).ToArray();
            }
            else
                return new ReadOnlyFileModel[0];
        }

        private void Clear()
        {
            if (Directory.Exists(_folder))
            {
                try
                {
                    new DirectoryInfo(_folder).Clean();
                }
                catch (Exception ex)
                {
                    _log.Warn(ex, "Could not clean data folder: " + _folder);
                }

            }
        }

        private void DeleteFile(string file)
        {
            File.Delete(Path.Combine(_folder, file));
        }

        private IFile GetFile(string file)
        {
            if (!file.IsFileNameValid())
                throw new ArgumentException($"Incorrect file name {file}");

            return new ReadOnlyFileModel(Path.Combine(_folder, file));
        }

        private void SaveFile(string file, byte[] bytes)
        {
            if (!file.IsFileNameValid())
                throw new ArgumentException($"Incorrect file name {file}");

            File.WriteAllBytes(Path.Combine(_folder, file), bytes);
        }

        private string GetFileReadPath(string file)
        {
            if (!file.IsFileNameValid())
                throw new ArgumentException($"Incorrect file name {file}");

            return Path.Combine(_folder, file);
        }

        private string GetFileWritePath(string file)
        {
            if (!file.IsFileNameValid())
                throw new ArgumentException($"Incorrect file name {file}");

            EnsureDirectoryCreated();

            return Path.Combine(_folder, file);
        }

        private void EnsureDirectoryCreated()
        {
            if (!Directory.Exists(_folder))
            {
                var dinfo = Directory.CreateDirectory(_folder);
            }
        }
    }
}
