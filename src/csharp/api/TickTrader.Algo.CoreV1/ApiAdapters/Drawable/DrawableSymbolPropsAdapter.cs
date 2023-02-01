using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class DrawableSymbolPropsAdapter : IDrawableSymbolProps
    {
        private readonly DrawableSymbolPropsInfo _info;


        public bool IsSupported => _info != null;

        public ushort Code
        {
            get => (ushort)_info.Code;
            set => _info.Code = value;
        }

        public int Size
        {
            get => _info.Size;
            set => _info.Size = value;
        }

        public Colors Color { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public string FontFamily
        {
            get => _info.FontFamily;
            set => _info.FontFamily = value;
        }

        public DrawableSymbolAnchor Anchor { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }


        public DrawableSymbolPropsAdapter(DrawableSymbolPropsInfo info)
        {
            _info = info;
        }
    }
}
