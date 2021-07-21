using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.Server
{
    public class PluginFileManagerModel
    {
        private readonly IActorRef _ref;


        public PluginFileManagerModel(IActorRef actor)
        {
            _ref = actor;
        }


        public Task<PluginFolderInfo> GetFolderInfo(PluginFolderInfoRequest request) => _ref.Ask<PluginFolderInfo>(request);

        public Task ClearFolder(ClearPluginFolderRequest request) => _ref.Ask(request);

        public Task DeleteFile(DeletePluginFileRequest request) => _ref.Ask(request);

        public Task<string> GetFileReadPath(DownloadPluginFileRequest request) => _ref.Ask<string>(request);

        public Task<string> GetFileWritePath(UploadPluginFileRequest request) => _ref.Ask<string>(request);
    }
}
