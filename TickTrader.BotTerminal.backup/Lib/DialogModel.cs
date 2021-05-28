using Caliburn.Micro;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    public class DialogModel : Screen, IWindowModel
    {
        private TaskCompletionSource<bool> _resultSrc = new TaskCompletionSource<bool>();

        public BoolVar IsValid { get; protected set; }
        public Task<bool> Result => _resultSrc.Task;

        public virtual void Ok()
        {
            _resultSrc.SetResult(true);
            TryCloseAsync();
        }

        public virtual void Cancel()
        {
            _resultSrc.SetResult(false);
            TryCloseAsync();
        }
    }
}
