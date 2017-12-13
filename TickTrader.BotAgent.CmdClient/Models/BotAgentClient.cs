using Machinarium.Qnil;
using TickTrader.Algo.Protocol;

namespace TickTrader.BotAgent.CmdClient
{
    public class BotAgentClient : IBotAgentClient
    {
        private DynamicList<AccountModelEntity> _accounts;
        private DynamicList<BotModelEntity> _bots;
        private DynamicList<PackageModelEntity> _packages;


        public IObservableListSource<AccountModelEntity> Accounts { get; }

        public IObservableListSource<BotModelEntity> Bots { get; }

        public IObservableListSource<PackageModelEntity> Packages { get; }


        public BotAgentClient()
        {
            _accounts = new DynamicList<AccountModelEntity>();
            _bots = new DynamicList<BotModelEntity>();
            _packages = new DynamicList<PackageModelEntity>();

            Accounts = _accounts.AsObservable();
            Bots = _bots.AsObservable();
            Packages = _packages.AsObservable();
        }


        public void InitAccountList(AccountListReportEntity report)
        {
            _accounts.Clear();
            foreach (var acc in report.Accounts)
            {
                _accounts.Add(acc);
            }
        }

        public void InitBotList(BotListReportEntity report)
        {
            _bots.Clear();
            foreach (var acc in report.Bots)
            {
                _bots.Add(acc);
            }
        }

        public void InitPackageList(PackageListReportEntity report)
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
                    _accounts.Add(update.Item);
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
                    _bots.Add(update.Item);
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
                    _packages.Add(update.Item);
                    break;
                case UpdateType.Updated:
                    break;
                case UpdateType.Removed:
                    break;
            }
        }
    }
}
