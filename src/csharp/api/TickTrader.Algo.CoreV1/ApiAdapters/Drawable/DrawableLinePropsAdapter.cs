﻿using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class DrawableLinePropsAdapter : IDrawableLineProps
    {
        private readonly DrawableLinePropsInfo _info;


        public bool IsSupported => _info != null;

        public Colors Color { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public int Thickness
        {
            get => _info.Thickness;
            set => _info.Thickness = value;
        }

        public LineStyles Style { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public bool RayLeft
        {
            get => _info.RayLeft;
            set => _info.RayLeft = value;
        }

        public bool RayRight
        {
            get => _info.RayRight;
            set => _info.RayRight = value;
        }

        public bool RayVertical
        {
            get => _info.RayVertical;
            set => _info.RayVertical = value;
        }


        public DrawableLinePropsAdapter(DrawableLinePropsInfo info)
        {
            _info = info;
        }
    }
}
