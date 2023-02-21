using System;
using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class DrawableObjectAnchorsAdapter : DrawablePropsChangedBase<DrawableObjectAnchorsList>, IDrawableObjectAnchors
    {
        private readonly List<AnchorAdapter> _anchors;


        public int Count => _anchors.Count;

        public IDrawableAnchorProps this[int index] => _anchors[index];


        public DrawableObjectAnchorsAdapter(DrawableObjectAnchorsList info, IDrawableChangedWatcher watcher) : base(info, watcher)
        {
            info = _info; // default .ctor will be called in base.ctor if null
            var n = info.Count;
            _anchors = new List<AnchorAdapter>(n);
            for (var i = 0; i < n; i++)
                _anchors.Add(new AnchorAdapter(info[i], OnChanged));
        }


        private class AnchorAdapter : IDrawableAnchorProps
        {
            private readonly DrawableAnchorPropsInfo _info;

            private Action _changedCallback;


            public DateTime Time
            {
                get => _info.Time.ToUtcDateTime();
                set
                {
                    _info.Time = new UtcTicks(value);
                    OnChanged();
                }
            }

            public double Price
            {
                get => _info.Price;
                set
                {
                    _info.Price = value;
                    OnChanged();
                }
            }


            public AnchorAdapter(DrawableAnchorPropsInfo info, Action changedCallback)
            {
                _info = info;
                _changedCallback = changedCallback;
            }


            public void IgnoreChanges() => _changedCallback = null;


            private void OnChanged() => _changedCallback?.Invoke();
        }
    }
}
