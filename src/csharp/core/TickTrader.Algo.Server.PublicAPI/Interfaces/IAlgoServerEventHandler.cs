using System.Collections.Generic;
using System.Threading.Tasks;

namespace TickTrader.Algo.Server.PublicAPI
{
    public interface IAlgoServerEventHandler
    {
        void AccessLevelChanged();

        Task<string> Get2FACode();


        #region Connection init

        void InitPackageList(List<PackageInfo> report);

        void InitAccountModelList(List<AccountModelInfo> report);

        void InitPluginModelList(List<PluginModelInfo> report);

        void SetApiMetadata(ApiMetadataInfo apiMetadata);

        void SetMappingsInfo(MappingCollectionInfo mappings);

        void SetSetupContext(SetupContextInfo setupContext);

        void InitUpdateSvcInfo(UpdateServiceInfo updateSvc);

        #endregion Connection init


        #region Updates

        void OnPackageUpdate(PackageUpdate update);

        void OnAccountModelUpdate(AccountModelUpdate update);

        void OnPluginModelUpdate(PluginModelUpdate update);


        void OnPackageStateUpdate(PackageStateUpdate packageState);

        void OnAccountStateUpdate(AccountStateUpdate accountState);

        void OnPluginStateUpdate(PluginStateUpdate pluginState);


        void OnPluginStatusUpdate(PluginStatusUpdate update);

        void OnPluginLogUpdate(PluginLogUpdate update);

        void OnAlertListUpdate(AlertListUpdate update);


        void OnUpdateSvcStateUpdate(UpdateServiceStateUpdate update);

        #endregion Updates
    }
}
