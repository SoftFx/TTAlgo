using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class DrawableLinePropsAdapter : DrawablePropsChangedBase, IDrawableLineProps
    {
        private readonly DrawableLinePropsInfo _info;


        public bool IsSupported => _info != null;

        public Colors Color
        {
            get => _info.ColorArgb.FromArgb();
            set
            {
                _info.ColorArgb = value.ToArgb();
                OnChanged();
            }
        }

        public int Thickness
        {
            get => _info.Thickness;
            set
            {
                _info.Thickness = value;
                OnChanged();
            }
        }

        public LineStyles Style
        {
            get => _info.Style.ToApiEnum();
            set
            {
                _info.Style = value.ToDomainEnum();
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


        public DrawableLinePropsAdapter(DrawableLinePropsInfo info, IDrawableChangedWatcher watcher) : base(watcher)
        {
            _info = info;
        }
    }
}
