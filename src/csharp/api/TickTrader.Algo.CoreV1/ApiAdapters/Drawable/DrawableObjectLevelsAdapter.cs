using System;
using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class DrawableObjectLevelsAdapter : DrawablePropsChangedBase<DrawableObjectLevelsList>, IDrawableObjectLevels
    {
        private readonly List<LevelAdapter> _levels = new List<LevelAdapter>(4);


        public int Count
        {
            get => _levels.Count;
            set => Resize(value);
        }

        public IDrawableLevelProps this[int index] => _levels[index];

        public Colors DefaultColor
        {
            get => _info.DefaultColorArgb.FromArgb();
            set
            {
                _info.DefaultColorArgb = value.ToArgb();
                OnChanged();
            }
        }

        public int DefaultLineThickness
        {
            get => _info.DefaultLineThickness;
            set
            {
                _info.DefaultLineThickness = value;
                OnChanged();
            }
        }

        public LineStyles DefaultLineStyle
        {
            get => _info.DefaultLineStyle.ToApiEnum();
            set
            {
                _info.DefaultLineStyle = value.ToDomainEnum();
                OnChanged();
            }
        }

        public string DefaultFontFamily
        {
            get => _info.DefaultFontFamily;
            set
            {
                _info.DefaultFontFamily = value;
                OnChanged();
            }
        }

        public int DefaultFontSize
        {
            get => _info.DefaultFontSize;
            set
            {
                _info.DefaultFontSize = value;
                OnChanged();
            }
        }


        public DrawableObjectLevelsAdapter(DrawableObjectLevelsList info, IDrawableChangedWatcher watcher)
            : base(info, watcher)
        {
            for (var i = 0; i < info.Count; i++)
            {
                _levels.Add(new LevelAdapter(info[i], OnChanged));
            }
        }


        private void Resize(int newCnt)
        {
            var cnt = _levels.Count;
            if (cnt == newCnt)
                return;

            if (cnt > newCnt)
            {
                for (var i = newCnt; i < cnt; i++)
                {
                    var index = _levels.Count - 1;
                    _info.Levels.RemoveAt(index);
                    _levels[index].IgnoreChanges();
                    _levels.RemoveAt(index);
                }
            }
            else
            {
                for (var i = cnt; i < newCnt; i++)
                {
                    var propsInfo = new DrawableLevelPropsInfo();
                    _info.Levels.Add(propsInfo);
                    _levels.Add(new LevelAdapter(propsInfo, OnChanged));
                }
            }

            OnChanged();
        }


        private class LevelAdapter : IDrawableLevelProps
        {
            private readonly DrawableLevelPropsInfo _info;

            private Action _changedCallback;


            public double Value
            {
                get => _info.Value;
                set
                {
                    _info.Value = value;
                    OnChanged();
                }
            }

            public string Text
            {
                get => _info.Text;
                set
                {
                    _info.Text = value;
                    OnChanged();
                }
            }

            public Colors Color
            {
                get => _info.ColorArgb.FromArgb();
                set
                {
                    _info.ColorArgb = value.ToArgb();
                    OnChanged();
                }
            }

            public int? LineThickness
            {
                get => _info.LineThickness;
                set
                {
                    _info.LineThickness = value;
                    OnChanged();
                }
            }

            public LineStyles? LineStyle
            {
                get => _info.LineStyle != Domain.Metadata.Types.LineStyle.UnknownLineStyle ? _info.LineStyle.ToApiEnum() : default(LineStyles?);
                set
                {
                    _info.LineStyle = value?.ToDomainEnum() ?? Domain.Metadata.Types.LineStyle.UnknownLineStyle;
                    OnChanged();
                }
            }

            public string FontFamily
            {
                get => _info.FontFamily;
                set
                {
                    _info.FontFamily = value;
                    OnChanged();
                }
            }

            public int? FontSize
            {
                get => _info.FontSize;
                set
                {
                    _info.FontSize = value;
                    OnChanged();
                }
            }


            public LevelAdapter(DrawableLevelPropsInfo info, Action changedCallback)
            {
                _info = info;
                _changedCallback = changedCallback;
            }


            public void IgnoreChanges() => _changedCallback = null;


            private void OnChanged() => _changedCallback?.Invoke();
        }
    }
}
