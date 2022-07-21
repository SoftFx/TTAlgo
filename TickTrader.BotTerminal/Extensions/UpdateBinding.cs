using System.Windows.Data;

namespace TickTrader.BotTerminal
{
    public class UpdateBind : Binding
    {
        public UpdateBind() : base()
        {
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Mode = BindingMode.OneWay;
        }

        public UpdateBind(string path) : base(path)
        {
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Mode = BindingMode.OneWay;
        }
    }
}
