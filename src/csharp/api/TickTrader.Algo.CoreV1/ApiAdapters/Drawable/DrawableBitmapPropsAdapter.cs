using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class DrawableBitmapPropsAdapter : DrawablePropsChangedBase, IDrawableBitmapProps
    {
        private readonly DrawableBitmapPropsInfo _info;


        public bool IsSupported => _info != null;

        public int OffsetX
        {
            get => _info.OffsetX;
            set
            {
                _info.OffsetX = value;
                OnChanged();
            }
        }

        public int OffsetY
        {
            get => _info.OffsetY;
            set
            {
                _info.OffsetY = value;
                OnChanged();
            }
        }

        public int Width
        {
            get => _info.Width;
            set
            {
                _info.Width = value;
                OnChanged();
            }
        }

        public int Height
        {
            get => _info.Height;
            set
            {
                _info.Height = value;
                OnChanged();
            }
        }

        public string FilePath
        {
            get => _info.FilePath;
            set
            {
                _info.FilePath = value;
                OnChanged();
            }
        }


        public DrawableBitmapPropsAdapter(DrawableBitmapPropsInfo info, IDrawableChangedWatcher watcher) : base(watcher)
        {
            _info = info;
        }
    }
}
