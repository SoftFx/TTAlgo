using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class DrawableBitmapPropsAdapter : DrawablePropsChangedBase<DrawableBitmapPropsInfo>, IDrawableBitmapProps
    {
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

        public uint Width
        {
            get => _info.Width;
            set
            {
                _info.Width = value;
                OnChanged();
            }
        }

        public uint Height
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


        public DrawableBitmapPropsAdapter(DrawableBitmapPropsInfo info, IDrawableChangedWatcher watcher)
            : base(info, watcher)
        {
        }
    }
}
