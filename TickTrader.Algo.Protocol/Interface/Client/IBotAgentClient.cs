using System.Collections.Generic;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Protocol
{
    public interface IBotAgentClient
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

        void UpdatePackage(UpdateInfo<PackageInfo> update);

        void UpdateAccount(UpdateInfo<AccountModelInfo> update);

        void UpdateBot(UpdateInfo<PluginModelInfo> update);

        void UpdatePackageState(UpdateInfo<PackageInfo> update);

        void UpdateAccountState(UpdateInfo<AccountModelInfo> update);

        void UpdateBotState(UpdateInfo<PluginModelInfo> update);

        #endregion Updates
    }
}
