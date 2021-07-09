using System.Collections.Generic;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.ServerControl
{
    public interface IAlgoServerClient
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

        void UpdatePackage(UpdateInfo.Types.UpdateType updateType, PackageInfo package);
        // void OnPackageUpdate(PackageUpdate update);

        void UpdateAccount(UpdateInfo.Types.UpdateType updateType, AccountModelInfo account);
        // void OnAccountUpdate(AccountModelUpdate update);

        void UpdateBot(UpdateInfo.Types.UpdateType updateType, PluginModelInfo plugin);
        // void OnPluginModelUpdate(PluginModelUpdate update);

        void UpdatePackageState(PackageStateUpdate packageState);
        // void OnPackageStateUpdate(PackageStateUpdate packageState);

        void UpdateAccountState(AccountStateUpdate accountState);
        // void OnAccountStateUpdate(AccountStateUpdate accountState);

        void UpdateBotState(PluginStateUpdate pluginState);
        // void OnPluginStateUpdate

        //void OnPluginStatusUpdate(PluginStatusUpdate update);

        //void OnPluginLogUpdate(PluginLogUpdate update); // params: string pluginId, LogRecord[] records

        //void OnAlertListUpdate(AlertListUpdate update); // params: AlertRecordInfo[]

        #endregion Updates
    }
}
