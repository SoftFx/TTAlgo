using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class DrawableSymbolPropsAdapter : DrawablePropsChangedBase, IDrawableSymbolProps
    {
        private readonly DrawableSymbolPropsInfo _info;


        public bool IsSupported => _info != null;

        public ushort Code
        {
            get => (ushort)_info.Code;
            set
            {
                _info.Code = value;
                OnChanged();
            }
        }

        public int Size
        {
            get => _info.Size;
            set
            {
                _info.Size = value;
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

        public DrawableSymbolAnchor Anchor { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }


        public DrawableSymbolPropsAdapter(DrawableSymbolPropsInfo info, IDrawableChangedWatcher watcher) : base(watcher)
        {
            _info = info;
        }
    }
}
