using Machinarium.Qnil;
using TickTrader.Algo.Protocol;

namespace TickTrader.BotAgent.CmdClient
{
    public class BotAgentClient : IBotAgentClient
    {
        private DynamicList<AccountModelEntity> _accounts;
        private DynamicList<PackageModelEntity> _packages;


        public IObservableListSource<AccountModelEntity> Accounts { get; }

        public IObservableListSource<PackageModelEntity> Packages { get; }


        public BotAgentClient()
        {
            _accounts = new DynamicList<AccountModelEntity>();
            _packages = new DynamicList<PackageModelEntity>();

            Accounts = _accounts.AsObservable();
            Packages = _packages.AsObservable();
        }


        public void SetAccountList(AccountListReportEntity report)
        {
            _accounts.Clear();
            foreach (var acc in report.Accounts)
            {
                _accounts.Add(acc);
            }
        }

        public void SetPackageList(PackageListReportEntity report)
        {
            _packages.Clear();
            foreach (var package in report.Packages)
            {
                _packages.Add(package);
            }
        }

        public void UpdateAccount(AccountModelUpdateEntity update)
        {
            switch (update.Type)
            {
                case UpdateType.Added:
                    _accounts.Add(update.NewItem);
                    break;
                case UpdateType.Updated:
                    break;
                case UpdateType.Removed:
                    break;
            }
        }

        public void UpdateBot(BotModelUpdateEntity update)
        {
            switch (update.Type)
            {
                case UpdateType.Added:
                    break;
                case UpdateType.Updated:
                    break;
                case UpdateType.Removed:
                    break;
            }
        }

        public void UpdatePackage(PackageModelUpdateEntity update)
        {
            switch (update.Type)
            {
                case UpdateType.Added:
                    _packages.Add(update.NewItem);
                    break;
                case UpdateType.Updated:
                    break;
                case UpdateType.Removed:
                    break;
            }
        }
    }
}
