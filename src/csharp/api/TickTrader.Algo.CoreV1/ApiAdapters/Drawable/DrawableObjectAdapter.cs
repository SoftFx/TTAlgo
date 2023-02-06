using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class DrawableObjectAdapter : IDrawableObject
    {
        private readonly DrawableObjectInfo _info;
        private readonly IDrawableUpdateSink _updateSink;

        private bool _isChanged;


        public string Name => _info.Name;

        public DrawableObjectType Type { get; } // local cache to reduce conversion costs

        public DateTime CreatedTime => _info.CreatedTime.ToUtcDateTime();

        public string OutputId => _info.OutputId;

        public bool IsBackground
        {
            get => _info.IsBackground;
            set => _info.IsBackground = value;
        }

        public bool IsHidden
        {
            get => _info.IsHidden;
            set => _info.IsHidden = value;
        }

        public bool IsSelectable
        {
            get => _info.IsSelectable;
            set => _info.IsSelectable = value;
        }

        public long ZIndex
        {
            get => _info.ZIndex;
            set => _info.ZIndex = value;
        }

        public string Tooltip
        {
            get => _info.Tooltip;
            set => _info.Tooltip = value;
        }

        public IDrawableLineProps Line { get; }

        public IDrawableShapeProps Shape { get; }

        public IDrawableSymbolProps Symbol { get; }

        public IDrawableTextProps Text => throw new NotImplementedException();

        public IDrawableObjectAnchors Anchors { get; }

        public IDrawableObjectLevels Levels => throw new NotImplementedException();

        public IDrawableControlProps Control => throw new NotImplementedException();

        public IDrawableBitmapProps Bitmap => throw new NotImplementedException();


        internal bool IsNew { get; private set; }


        public DrawableObjectAdapter(DrawableObjectInfo info, DrawableObjectType type, IDrawableUpdateSink updateSink)
        {
            _info = info;
            Type = type;
            _updateSink = updateSink;
            IsNew = true;

            Line = new DrawableLinePropsAdapter(info.LineProps);
            Shape = new DrawableShapePropsAdapter(info.ShapeProps);
            Symbol = new DrawableSymbolPropsAdapter(info.SymbolProps);
            Anchors = new DrawableObjectAnchorsAdapter(info.Anchors);
        }


        public void PushChanges() => PushChangesInternal();


        internal void PushChangesInternal()
        {
            if (!IsNew && !_isChanged)
                return;

            var infoCopy = _info.Clone();

            DrawableObjectUpdate upd = default;
            if (IsNew)
            {
                IsNew = false;
                upd = DrawableObjectUpdate.Added(infoCopy);
            }
            else
            {
                upd = DrawableObjectUpdate.Removed(Name);
            }

            IsNew = false;
            _isChanged = false;

            _updateSink.Send(upd);
        }
    }
}
