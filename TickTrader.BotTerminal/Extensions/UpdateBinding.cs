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


    public class BoolToVisBind : UpdateBind
    {
        private readonly BooleanToVisibilityConverter _converter = new();

        public BoolToVisBind() : base()
        {
            Converter = _converter;
        }

        public BoolToVisBind(string path) : base(path)
        {
            Converter = _converter;
        }
    }


    public class InvBoolToVisBind : UpdateBind
    {
        private readonly BooleanToVisibilityConverter _converter = new()
        {
            Invert = true,
        };


        public InvBoolToVisBind() : base()
        {
            Converter = _converter;
        }

        public InvBoolToVisBind(string path) : base(path)
        {
            Converter = _converter;
        }
    }
}
