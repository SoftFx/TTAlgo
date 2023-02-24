using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class DrawableSpecialPropsAdapter : DrawablePropsChangedBase<DrawableSpecialPropsInfo>, IDrawableSpecialProps
    {
        public double? Angle
        {
            get => _info.Angle;
            set
            {
                _info.Angle = value;
                OnChanged();
            }
        }

        public double? Scale
        {
            get => _info.Scale;
            set
            {
                _info.Scale = value;
                OnChanged();
            }
        }

        public double? Deviation
        {
            get => _info.Deviation;
            set
            {
                _info.Deviation = value;
                OnChanged();
            }
        }

        public DrawableLineRayMode RayMode
        {
            get => _info.RayMode.ToApiEnum();
            set
            {
                _info.RayMode = value.ToDomainEnum();
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

        public bool FiboArcsFullEllipse
        {
            get => _info.FiboArcsFullEllipse;
            set
            {
                _info.FiboArcsFullEllipse = value;
                OnChanged();
            }
        }

        public DrawablePositionMode AnchorPosition
        {
            get => _info.AnchorPosition.ToApiEnum();
            set
            {
                _info.AnchorPosition = value.ToDomainEnum();
                OnChanged();
            }
        }

        public DrawableGannDirection GannDirection
        {
            get => _info.GannDirection.ToApiEnum();
            set
            {
                _info.GannDirection = value.ToDomainEnum();
                OnChanged();
            }
        }


        public DrawableSpecialPropsAdapter(DrawableSpecialPropsInfo info, IDrawableChangedWatcher watcher) : base(info, watcher)
        {
        }
    }
}
