using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public abstract class Indicator : AlgoPlugin
    {
        private string _loggerPrefix;

        public Indicator()
        {
        }

        /// <summary>
        /// True if Calculate() is called to recalculate last bar/quote. Inputs and outputs are not shifted in this case.
        /// False if Calculate() is called on a new bar/quote. Inputs and outputs are shifted, fresh data is placed at zero index.
        /// </summary>
        [Obsolete("Will be removed in future. Override Calculate(bool isNewBar) instead")]
        protected bool IsUpdate { get; private set; }


        [Obsolete("Will be removed in future. Override Calculate(bool isNewBar) instead")]
        protected virtual void Calculate() { }
        protected virtual void Calculate(bool isNewBar) { }

        internal void InvokeCalculate(bool isUpdate)
        {
            IsUpdate = isUpdate;
            Calculate();
            Calculate(!isUpdate);
        }

        internal override void InvokeInit()
        {
            _loggerPrefix = $"{Id} ({Symbol.Name}, {TimeFrame})";
            base.InvokeInit();
        }


        #region Logger

        public void Print(string msg)
        {
            context.Logger.Print(AddLoggerPrefix(msg));
        }

        public void Print(string msg, params object[] parameters)
        {
            context.Logger.Print(AddLoggerPrefix(msg), parameters);
        }

        public void PrintError(string msg)
        {
            context.Logger.PrintError(AddLoggerPrefix(msg));
        }

        public void PrintError(string msg, params object[] parameters)
        {
            context.Logger.PrintError(AddLoggerPrefix(msg), parameters);
        }

        private string AddLoggerPrefix(string msg)
        {
            return $"{_loggerPrefix}: {msg}";
        }

        #endregion
    }
}
