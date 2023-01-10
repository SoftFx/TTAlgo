using System.Windows.Data;

namespace TickTrader.BotTerminal
{
    public class UpdateBind : Binding
    {
        public UpdateBind() : base()
        {
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Mode = BindingMode.TwoWay;
        }

        public UpdateBind(string path) : base(path)
        {
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Mode = BindingMode.TwoWay;
        }
    }
}
