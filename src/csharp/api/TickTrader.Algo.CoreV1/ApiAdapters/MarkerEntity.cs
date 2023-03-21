﻿using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    public class MarkerEntity : Marker, IFixedEntry<Marker>
    {
        private static Action<Marker> EmptyHandler = e => { };

        private double _y = double.NaN;
        private Colors _color = Colors.Auto;
        private string _text;
        private MarkerIcons _icon;
        private ushort _iconCode;
        private Action<Marker> _changed = EmptyHandler;


        public Colors Color
        {
            get => _color;
            set
            {
                _color = value;
                Changed(this);
            }
        }

        public string DisplayText
        {
            get => _text;
            set
            {
                _text = Normalize(value);
                Changed(this);
            }
        }

        public MarkerIcons Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                Changed(this);
            }
        }

        public double Y
        {
            get => _y;
            set
            {
                _y = value;
                Changed(this);
            }
        }

        public ushort IconCode
        {
            get => _iconCode;
            set
            {
                _iconCode = value;
                Changed(this);
            }
        }

        public Action<Marker> Changed
        {
            get => _changed;
            set
            {
                if (_changed == null)
                    throw new InvalidOperationException("Changed cannot be null!");
                _changed = value;
            }
        }


        public void Clear()
        {
            Reset();
            Changed(this);
        }

        public void CopyFrom(Marker val)
        {
            if (val == null)
                Reset();
            else
            {
                _y = val.Y;
                _color = val.Color;
                _text = Normalize(val.DisplayText);
                _icon = val.Icon;
            }
            Changed(this);
        }

        public MarkerInfo GetInfo()
        {
            return new MarkerInfo
            {
                DisplayText = DisplayText,
                ColorArgb = Color.ToArgb(),
                Icon = Icon.ToDomainEnum(),
                IconCode = IconCode,
            };
        }

        public static MarkerEntity From(MarkerInfo info)
        {
            return new MarkerEntity
            {
                _color = info.ColorArgb.FromArgb(),
                _text = info.DisplayText,
                _icon = info.Icon.ToApiEnum(),
                _iconCode = (ushort)info.IconCode,
            };
        }

        // seems redundant. Needs additional verification
        private string Normalize(string input)
        {
            if (input == null)
                return string.Empty;
            return input;
        }

        private void Reset()
        {
            _y = double.NaN;
            _color = Colors.Auto;
            _text = string.Empty;
            _icon = MarkerIcons.Circle;
            _iconCode = 0;
        }
    }
}
