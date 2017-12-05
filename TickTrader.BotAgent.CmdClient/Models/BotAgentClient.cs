using Machinarium.Qnil;
using TickTrader.Algo.Protocol;

namespace TickTrader.BotAgent.CmdClient
{
    public class BotAgentClient : IBotAgentClient
    {
        private DynamicList<AccountModelEntity> _accounts;


        public IObservableListSource<AccountModelEntity> Accounts { get; }


        public BotAgentClient()
        {
            _accounts = new DynamicList<AccountModelEntity>();

            Accounts = _accounts.AsObservable();
        }


        public void SetAccountList(AccountListReportEntity report)
        {
            _accounts.Clear();
            foreach(var acc in report.Accounts)
            {
                _accounts.Add(acc);
            }
        }
    }
}
