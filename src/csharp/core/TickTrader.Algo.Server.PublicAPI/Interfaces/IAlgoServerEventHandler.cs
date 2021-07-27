using System.Collections.Generic;


namespace TickTrader.Algo.Server.PublicAPI
{
    public interface IAlgoServerEventHandler
    {
        void AccessLevelChanged();


        #region Connection init

        void InitPackageList(List<PackageInfo> report);

        void InitAccountList(List<AccountModelInfo> report);

        void InitBotList(List<PluginModelInfo> report);

        void SetApiMetadata(ApiMetadataInfo apiMetadata);

        void SetMappingsInfo(MappingCollectionInfo mappings);

        void SetSetupContext(SetupContextInfo setupContext);

        #endregion Connection init


        #region Updates

        void OnPackageUpdate(PackageUpdate update);

        void OnAccountUpdate(AccountModelUpdate update);

        void OnPluginModelUpdate(PluginModelUpdate update);


        void OnPackageStateUpdate(PackageStateUpdate packageState);

        void OnAccountStateUpdate(AccountStateUpdate accountState);

        void OnPluginStateUpdate(PluginStateUpdate pluginState);


        void OnPluginStatusUpdate(PluginStatusUpdate update);

        void OnPluginLogUpdate(PluginLogUpdate update);

        void OnAlertListUpdate(AlertListUpdate update);

        #endregion Updates
    }
}
