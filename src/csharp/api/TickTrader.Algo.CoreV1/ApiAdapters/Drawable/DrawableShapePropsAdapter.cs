using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class DrawableShapePropsAdapter : DrawablePropsChangedBase<DrawableShapePropsInfo>, IDrawableShapeProps
    {
        public Colors BorderColor
        {
            get => _info.BorderColorArgb.FromArgb();
            set
            {
                _info.BorderColorArgb = value.ToArgb();
                OnChanged();
            }
        }

        public ushort BorderThickness
        {
            get => (ushort)_info.BorderThickness;
            set
            {
                _info.BorderThickness = value;
                OnChanged();
            }
        }

        public LineStyles BorderStyle
        {
            get => _info.BorderStyle.ToApiEnum();
            set
            {
                _info.BorderStyle = value.ToDomainEnum();
                OnChanged();
            }
        }

        public Colors FillColor
        {
            get => _info.FillColorArgb.FromArgb();
            set
            {
                _info.FillColorArgb = value.ToArgb();
                OnChanged();
            }
        }


        public DrawableShapePropsAdapter(DrawableShapePropsInfo info, IDrawableChangedWatcher watcher)
            : base(info, watcher)
        {
        }
    }
}
