using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TickTrader.BotTerminal
{
    public class ValidationBinding : Binding
    {
        public ValidationBinding() : base()
        {
            ValidatesOnDataErrors = true;
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Mode = BindingMode.TwoWay;
        }

        public ValidationBinding(string path) : base(path)
        {
            ValidatesOnDataErrors = true;
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Mode = BindingMode.TwoWay;
        }
    }
}
