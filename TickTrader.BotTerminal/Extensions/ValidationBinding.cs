using System.Windows.Data;

namespace TickTrader.BotTerminal
{
    public class ValidationBinding : UpdateBind
    {
        public ValidationBinding() : base()
        {
            ValidatesOnDataErrors = true;
            ValidatesOnNotifyDataErrors = true;

            Mode = BindingMode.TwoWay;
        }

        public ValidationBinding(string path) : base(path)
        {
            ValidatesOnDataErrors = true;
            ValidatesOnNotifyDataErrors = true;

            Mode = BindingMode.TwoWay;
        }
    }
}
