using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.Server
{
    public class AlgoServerFacade
    {
        private readonly AlgoServer _server;


        public AlgoServerFacade(AlgoServer server)
        {
            _server = server;
        }


        public Task<PackageListSnapshot> GetPackageSnapshot() => _server.EventBus.GetPackageSnapshot();
        public Task<bool> PackageWithNameExists(string pkgName) => _server.PkgStorage.PackageWithNameExists(pkgName);
        public Task UploadPackage(UploadPackageRequest request, string pkgFilePath) => _server.PkgStorage.UploadPackage(request, pkgFilePath);
        public Task<byte[]> GetPackageBinary(string packageId) => _server.PkgStorage.GetPackageBinary(packageId);
        public Task RemovePackage(RemovePackageRequest request) => _server.PkgStorage.RemovePackage(request);
        public Task<MappingCollectionInfo> GetMappingsInfo() => throw new System.NotImplementedException();

        public Task<AccountListSnapshot> GetAccounts() => _server.EventBus.GetAccountSnapshot();
        public Task AddAccount(AddAccountRequest request) => _server.Accounts.Add(request);
        public Task ChangeAccount(ChangeAccountRequest request) => _server.Accounts.Change(request);
        public Task RemoveAccount(RemoveAccountRequest request) => _server.Accounts.Remove(request);
        public Task<ConnectionErrorInfo> TestAccount(TestAccountRequest request) => _server.Accounts.Test(request);
        public Task<ConnectionErrorInfo> TestCreds(TestAccountCredsRequest request) => _server.Accounts.TestCreds(request);
        public Task<AccountMetadataInfo> GetAccountMetadata(AccountMetadataRequest request) => _server.Accounts.GetMetadata(request);

        public Task<PluginListSnapshot> GetPlugins() => _server.EventBus.GetPluginSnapshot();
        public Task<bool> PluginExists(string pluginId) => _server.Plugins.PluginExists(pluginId);
        public Task AddPlugin(AddPluginRequest request) => _server.Plugins.Add(request);
        public Task UpdatePluginConfig(ChangePluginConfigRequest request) => _server.Plugins.UpdateConfig(request);
        public Task RemovePlugin(RemovePluginRequest request) => _server.Plugins.Remove(request);
        public Task StartPlugin(StartPluginRequest request) => _server.Plugins.StartPlugin(request);
        public Task StopPlugin(StopPluginRequest request) => _server.Plugins.StopPlugin(request);
        public Task<PluginModelInfo> GetPluginInfo(string pluginId) => _server.EventBus.GetPluginInfo(pluginId);

        Task<object> GetPluginFolder(string pluginId, PluginFolderInfo.Types.PluginFolderId folderId) => throw new System.NotImplementedException();
    }
}
