using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class DrawableShapePropsAdapter : DrawablePropsChangedBase, IDrawableShapeProps
    {
        private readonly DrawableShapePropsInfo _info;


        public bool IsSupported => _info != null;

        public Colors BorderColor { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public int BorderThickness
        {
            get => _info.BorderThickness;
            set
            {
                _info.BorderThickness = value;
                OnChanged();
            }
        }

        public LineStyles BorderStyle { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public bool Fill
        {
            get => _info.Fill;
            set
            {
                _info.Fill = value;
                OnChanged();
            }
        }

        public Colors FillColor { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }


        public DrawableShapePropsAdapter(DrawableShapePropsInfo info, IDrawableChangedWatcher watcher) : base(watcher)
        {
            _info = info;
        }
    }
}
