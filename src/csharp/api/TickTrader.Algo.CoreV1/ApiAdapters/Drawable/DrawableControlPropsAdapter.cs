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

        public uint? Width
        {
            get => _info.Width;
            set
            {
                _info.Width = value;
                OnChanged();
            }
        }

        public uint? Height
        {
            get => _info.Height;
            set
            {
                _info.Height = value;
                OnChanged();
            }
        }

        public DrawableControlZeroPosition ZeroPosistion
        {
            get => _info.ZeroPosition.ToApiEnum();
            set
            {
                _info.ZeroPosition = value.ToDomainEnum();
                OnChanged();
            }
        }

        public DrawablePositionMode ContentAlignment
        {
            get => _info.ContentAlignment.ToApiEnum();
            set
            {
                _info.ContentAlignment = value.ToDomainEnum();
                OnChanged();
            }
        }


        public DrawableControlPropsAdapter(DrawableControlPropsInfo info, IDrawableChangedWatcher watcher)
            : base(info, watcher)
        {
        }
    }
}
