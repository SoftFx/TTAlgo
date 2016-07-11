using Caliburn.Micro;
using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Machinarium.Qnil;

namespace TickTrader.BotTerminal
{
    internal class MarketOrderPageViewModel : Screen, IOpenOrderDialogPage, IDataErrorInfo
    {
        public MarketOrderPageViewModel(OpenOrderDialogViewModel orderViewModel)
        {
            this.Order = orderViewModel;

            DisplayName = "Market order";
        }

        public string this[string columnName]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Error
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #region Bindable Properties

        public OpenOrderDialogViewModel Order { get; private set; }

        #endregion
    }
}