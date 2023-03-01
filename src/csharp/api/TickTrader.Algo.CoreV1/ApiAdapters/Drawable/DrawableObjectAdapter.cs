using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal interface IDrawableChangedWatcher
    {
        void OnChanged();
    }


    internal class DrawableObjectAdapter : IDrawableObject, IDrawableChangedWatcher
    {
        private readonly DrawableObjectInfo _info;
        private readonly IDrawableUpdateSink _updateSink;


        public string Name => _info.Name;

        public DrawableObjectType Type { get; } // local cache to reduce conversion costs

        public DateTime CreatedTime => _info.CreatedTime.ToUtcDateTime();

        public OutputTargets TargetWindow => _info.TargetWindow.ToApiEnum();

        public bool IsBackground
        {
            get => _info.IsBackground;
            set
            {
                _info.IsBackground = value;
                OnChanged();
            }
        }

        public bool IsHidden
        {
            get => _info.IsHidden;
            set
            {
                _info.IsHidden = value;
                OnChanged();
            }
        }

        public int ZIndex
        {
            get => _info.ZIndex;
            set
            {
                _info.ZIndex = value;
                OnChanged();
            }
        }

        public string Tooltip
        {
            get => _info.Tooltip;
            set
            {
                _info.Tooltip = value;
                OnChanged();
            }
        }

        public DrawableObjectVisibility Visibility
        {
            get => (DrawableObjectVisibility)_info.VisibilityBitmask;
            set
            {
                _info.VisibilityBitmask = (uint)value;
                OnChanged();
            }
        }

        public IDrawableLineProps Line { get; }

        public IDrawableShapeProps Shape { get; }

        public IDrawableSymbolProps Symbol { get; }

        public IDrawableTextProps Text { get; }

        public IDrawableObjectAnchors Anchors { get; }

        public IDrawableObjectLevels Levels { get; }

        public IDrawableControlProps Control { get; }

        public IDrawableBitmapProps Bitmap { get; }

        public IDrawableSpecialProps Special { get; }


        internal bool IsNew { get; private set; }

        internal bool IsChanged { get; private set; }

        internal bool IsRemoved { get; private set; }


        public DrawableObjectAdapter(DrawableObjectInfo info, DrawableObjectType type, IDrawableUpdateSink updateSink)
        {
            _info = info;
            Type = type;
            _updateSink = updateSink;
            IsNew = true;

            Line = new DrawableLinePropsAdapter(info.LineProps, this);
            Shape = new DrawableShapePropsAdapter(info.ShapeProps, this);
            Symbol = new DrawableSymbolPropsAdapter(info.SymbolProps, this);
            Text = new DrawableTextPropsAdapter(info.TextProps, this);
            Anchors = new DrawableObjectAnchorsAdapter(info.Anchors, this);
            Levels = new DrawableObjectLevelsAdapter(info.Levels, this);
            Control = new DrawableControlPropsAdapter(info.ControlProps, this);
            Bitmap = new DrawableBitmapPropsAdapter(info.BitmapProps, this);
            Special = new DrawableSpecialPropsAdapter(info.SpecialProps, this);
        }


        public override string ToString()
        {
            return $"{Name} ({Type})";
        }

        public void PushChanges() => PushChangesInternal();


        internal void PushChangesInternal()
        {
            if (IsRemoved || (!IsNew && !IsChanged))
                return;

            var infoCopy = _info.Clone();

            DrawableCollectionUpdate upd = default;
            if (IsNew)
            {
                IsNew = false;
                upd = DrawableCollectionUpdate.Added(infoCopy);
            }
            else
            {
                upd = DrawableCollectionUpdate.Updated(infoCopy);
            }

            IsNew = false;
            IsChanged = false;

            _updateSink.Send(upd);
        }

        internal void OnRemoved() => IsRemoved = true;


        private void OnChanged() => IsChanged = true;

        void IDrawableChangedWatcher.OnChanged() => OnChanged();
    }
}
