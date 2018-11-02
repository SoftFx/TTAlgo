using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage
{
    internal struct AutoTransactionScope : IDisposable
    {
        private ITransaction _transaction;
        private bool _isAutoTransaction;

        public AutoTransactionScope(ITransaction extTransaction, Func<ITransaction> transactionFactory)
        {
            _transaction = extTransaction ?? transactionFactory();
            _isAutoTransaction = extTransaction == null;
        }

        public ITransaction Transaction => _transaction;

        public void Commit()
        {
            if (_isAutoTransaction)
                _transaction.Commit();
        }

        public void Dispose()
        {
            if (_isAutoTransaction)
                _transaction.Dispose();
        }
    }
}
