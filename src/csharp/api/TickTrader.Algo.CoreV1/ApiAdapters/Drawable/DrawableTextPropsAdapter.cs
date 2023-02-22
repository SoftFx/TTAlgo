using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class DrawableTextPropsAdapter : DrawablePropsChangedBase<DrawableTextPropsInfo>, IDrawableTextProps
    {
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

        public ushort FontSize
        {
            get => (ushort)_info.FontSize;
            set
            {
                _info.FontSize = value;
                OnChanged();
            }
        }


        public DrawableTextPropsAdapter(DrawableTextPropsInfo info, IDrawableChangedWatcher watcher)
            : base(info, watcher)
        {
        }
    }
}
