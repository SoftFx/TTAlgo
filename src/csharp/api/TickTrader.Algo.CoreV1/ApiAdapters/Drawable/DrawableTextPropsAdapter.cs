using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class DrawableTextPropsAdapter : DrawablePropsChangedBase, IDrawableTextProps
    {
        private readonly DrawableTextPropsInfo _info;


        public bool IsSupported => _info != null;

        public string Content
        {
            get => _info.Content;
            set
            {
                _info.Content = value;
                OnChanged();
            }
        }

        public Colors Color
        {
            get => _info.ColorArgb.FromArgb();
            set
            {
                _info.ColorArgb = value.ToArgb();
                OnChanged();
            }
        }

        public string FontFamily
        {
            get => _info.FontFamily;
            set
            {
                _info.FontFamily = value;
                OnChanged();
            }
        }

        public int FontSize
        {
            get => _info.FontSize;
            set
            {
                _info.FontSize = value;
                OnChanged();
            }
        }

        public double Angle
        {
            get => _info.Angle;
            set
            {
                _info.Angle = value;
                OnChanged();
            }
        }


        public DrawableTextPropsAdapter(DrawableTextPropsInfo info, IDrawableChangedWatcher watcher) : base(watcher)
        {
            _info = info;
        }
    }
}
