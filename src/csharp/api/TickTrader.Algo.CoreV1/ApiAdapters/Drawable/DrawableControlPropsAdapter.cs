using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class DrawableControlPropsAdapter : DrawablePropsChangedBase<DrawableControlPropsInfo>, IDrawableControlProps
    {
        public int X
        {
            get => _info.X;
            set
            {
                _info.X = value;
                OnChanged();
            }
        }

        public int Y
        {
            get => _info.Y;
            set
            {
                _info.Y = value;
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

        public DrawableControlAnchor Anchor
        {
            get => _info.Anchor.ToApiEnum();
            set
            {
                _info.Anchor = value.ToDomainEnum();
                OnChanged();
            }
        }


        public DrawableControlPropsAdapter(DrawableControlPropsInfo info, IDrawableChangedWatcher watcher)
            : base(info, watcher)
        {
        }
    }
}
