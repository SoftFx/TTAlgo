using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class DrawableSpecialPropsAdapter : DrawablePropsChangedBase<DrawableSpecialPropsInfo>, IDrawableSpecialProps
    {
        public double Angle
        {
            get => _info.Angle ?? 0.0;
            set
            {
                _info.Angle = value == 0.0 ? default(double?) : value;
                OnChanged();
            }
        }

        public double Scale
        {
            get => _info.Scale ?? 0.0;
            set
            {
                _info.Scale = value == 0.0 ? default(double?) : value;
                OnChanged();
            }
        }

        public double Deviation
        {
            get => _info?.Deviation ?? 0.0;
            set
            {
                _info.Deviation = value == 0.0 ? default(double?) : value;
                OnChanged();
            }
        }

        public bool RayLeft
        {
            get => _info.RayLeft;
            set
            {
                _info.RayLeft = value;
                OnChanged();
            }
        }

        public bool RayRight
        {
            get => _info.RayRight;
            set
            {
                _info.RayRight = value;
                OnChanged();
            }
        }

        public bool RayVertical
        {
            get => _info.RayVertical;
            set
            {
                _info.RayVertical = value;
                OnChanged();
            }
        }

        public bool Fill
        {
            get => _info.Fill;
            set
            {
                _info.Fill = value;
                OnChanged();
            }
        }

        public bool ButtonState
        {
            get => _info.ButtonState;
            set
            {
                _info.ButtonState = value;
                OnChanged();
            }
        }


        public DrawableSpecialPropsAdapter(DrawableSpecialPropsInfo info, IDrawableChangedWatcher watcher) : base(info, watcher)
        {
        }
    }
}
