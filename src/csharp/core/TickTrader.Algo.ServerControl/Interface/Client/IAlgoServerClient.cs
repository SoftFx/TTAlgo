using System.Collections.Generic;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.ServerControl
{
    public interface IAlgoServerClient
    {
        void AccessLevelChanged();

        #region Initialization

        void InitPackageList(List<PackageInfo> report);

        void InitAccountList(List<AccountModelInfo> report);

        void InitBotList(List<PluginModelInfo> report);

        void SetApiMetadata(ApiMetadataInfo apiMetadata);

        void SetMappingsInfo(MappingCollectionInfo mappings);

        void SetSetupContext(SetupContextInfo setupContext);

        #endregion Initialization


        #region Updates

        void UpdatePackage(UpdateInfo.Types.UpdateType updateType, PackageInfo package);

        void UpdateAccount(UpdateInfo.Types.UpdateType updateType, AccountModelInfo account);

        void UpdateBot(UpdateInfo.Types.UpdateType updateType, PluginModelInfo plugin);

        void UpdatePackageState(PackageStateUpdate packageState);

        void UpdateAccountState(AccountStateUpdate accountState);

        void UpdateBotState(PluginStateUpdate pluginState);

        #endregion Updates
    }
}
