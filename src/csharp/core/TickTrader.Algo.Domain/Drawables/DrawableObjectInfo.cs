namespace TickTrader.Algo.Domain
{
    public partial class DrawableObjectInfo
    {
        public UtcTicks CreatedTime
        {
            get => new UtcTicks(CreatedTimeRaw);
            set => CreatedTimeRaw = value.Value;
        }

        public DrawableVisibility Visibility
        {
            get => (DrawableVisibility)VisibilityBitmask;
            set => VisibilityBitmask = (uint)value;
        }


        public DrawableObjectInfo(string name, Drawable.Types.ObjectType type)
        {
            Name = name;
            Type = type;
            CreatedTime = UtcTicks.Now;
            TargetWindow = Metadata.Types.OutputTarget.Overlay;
            VisibilityBitmask = (uint)DrawableVisibility.AllTimeframes;
            IsHidden = true;

            InitFields();
        }


        private void InitFields()
        {
            var type = Type;
            LineProps = SupportsLineProps(type) ? new DrawableLinePropsInfo() : default;
            ShapeProps = SupportsShapeProps(type) ? new DrawableShapePropsInfo() : default;
            SymbolProps = SupportsSymbolProps(type) ? new DrawableSymbolPropsInfo() : default;
            TextProps = SupportsTextProps(type) ? new DrawableTextPropsInfo() : default;
            ControlProps = SupportsControlProps(type) ? new DrawableControlPropsInfo() : default;
            BitmapProps = SupportsBitmapProps(type) ? new DrawableBitmapPropsInfo() : default;
            SpecialProps = SupportsSpecialProps(type) ? new DrawableSpecialPropsInfo() : default;
            Levels = SupportsLevelsList(type) ? new DrawableObjectLevelsList(0) : default;
            var anchorsCnt = GetAnchorListSize(type);
            Anchors = anchorsCnt != 0 ? new DrawableObjectAnchorsList(anchorsCnt) : default;
        }

        public static bool SupportsLineProps(Drawable.Types.ObjectType type)
        {
            switch (type)
            {
                case Drawable.Types.ObjectType.VerticalLine:
                case Drawable.Types.ObjectType.HorizontalLine:
                case Drawable.Types.ObjectType.TrendLine:
                case Drawable.Types.ObjectType.Cycles:
                case Drawable.Types.ObjectType.LinRegChannel:
                case Drawable.Types.ObjectType.StdDevChannel:
                case Drawable.Types.ObjectType.EquidistantChannel:
                case Drawable.Types.ObjectType.GannLine:
                case Drawable.Types.ObjectType.GannFan:
                case Drawable.Types.ObjectType.GannGrid:
                case Drawable.Types.ObjectType.FiboFan:
                case Drawable.Types.ObjectType.FiboArcs:
                case Drawable.Types.ObjectType.FiboChannel:
                case Drawable.Types.ObjectType.FiboRetracement:
                case Drawable.Types.ObjectType.FiboTimeZones:
                case Drawable.Types.ObjectType.FiboExpansion:
                case Drawable.Types.ObjectType.AndrewsPitchfork:
                    return true;
                default: return false;
            }
        }

        public static bool SupportsShapeProps(Drawable.Types.ObjectType type)
        {
            switch (type)
            {
                case Drawable.Types.ObjectType.Rectangle:
                case Drawable.Types.ObjectType.Triangle:
                case Drawable.Types.ObjectType.Ellipse:
                case Drawable.Types.ObjectType.RectangleControl:
                case Drawable.Types.ObjectType.EditControl:
                case Drawable.Types.ObjectType.ButtonControl:
                case Drawable.Types.ObjectType.TextBlockControl:
                    return true;
                default: return false;
            }
        }

        public static bool SupportsSymbolProps(Drawable.Types.ObjectType type)
        {
            switch (type)
            {
                case Drawable.Types.ObjectType.Symbol:
                    return true;
                default: return false;
            }
        }

        public static bool SupportsTextProps(Drawable.Types.ObjectType type)
        {
            switch (type)
            {
                case Drawable.Types.ObjectType.Text:
                case Drawable.Types.ObjectType.LabelControl:
                case Drawable.Types.ObjectType.EditControl:
                case Drawable.Types.ObjectType.ButtonControl:
                case Drawable.Types.ObjectType.TextBlockControl:
                    return true;
                default: return false;
            }
        }

        public static bool SupportsControlProps(Drawable.Types.ObjectType type)
        {
            switch (type)
            {
                case Drawable.Types.ObjectType.LabelControl:
                case Drawable.Types.ObjectType.RectangleControl:
                case Drawable.Types.ObjectType.EditControl:
                case Drawable.Types.ObjectType.ButtonControl:
                case Drawable.Types.ObjectType.BitmapControl:
                case Drawable.Types.ObjectType.TextBlockControl:
                    return true;
                default: return false;
            }
        }

        public static bool SupportsBitmapProps(Drawable.Types.ObjectType type)
        {
            switch (type)
            {
                case Drawable.Types.ObjectType.Bitmap:
                case Drawable.Types.ObjectType.BitmapControl:
                    return true;
                default: return false;
            }
        }

        public static bool SupportsLevelsList(Drawable.Types.ObjectType type)
        {
            switch (type)
            {
                case Drawable.Types.ObjectType.Levels:
                case Drawable.Types.ObjectType.FiboFan:
                case Drawable.Types.ObjectType.FiboArcs:
                case Drawable.Types.ObjectType.FiboChannel:
                case Drawable.Types.ObjectType.FiboRetracement:
                case Drawable.Types.ObjectType.FiboTimeZones:
                case Drawable.Types.ObjectType.FiboExpansion:
                case Drawable.Types.ObjectType.AndrewsPitchfork:
                    return true;
                default: return false;
            }
        }

        public static int GetAnchorListSize(Drawable.Types.ObjectType type)
        {
            switch (type)
            {
                case Drawable.Types.ObjectType.VerticalLine:
                case Drawable.Types.ObjectType.HorizontalLine:
                case Drawable.Types.ObjectType.Symbol:
                case Drawable.Types.ObjectType.Text:
                case Drawable.Types.ObjectType.Bitmap:
                    return 1;
                case Drawable.Types.ObjectType.TrendLine:
                case Drawable.Types.ObjectType.Rectangle:
                case Drawable.Types.ObjectType.Ellipse:
                case Drawable.Types.ObjectType.Cycles:
                case Drawable.Types.ObjectType.LinRegChannel:
                case Drawable.Types.ObjectType.StdDevChannel:
                case Drawable.Types.ObjectType.GannLine:
                case Drawable.Types.ObjectType.GannFan:
                case Drawable.Types.ObjectType.GannGrid:
                case Drawable.Types.ObjectType.FiboFan:
                case Drawable.Types.ObjectType.FiboArcs:
                case Drawable.Types.ObjectType.FiboRetracement:
                case Drawable.Types.ObjectType.FiboTimeZones:
                    return 2;
                case Drawable.Types.ObjectType.Triangle:
                case Drawable.Types.ObjectType.EquidistantChannel:
                case Drawable.Types.ObjectType.FiboChannel:
                case Drawable.Types.ObjectType.FiboExpansion:
                case Drawable.Types.ObjectType.AndrewsPitchfork:
                    return 3;
                default: return 0;
            }
        }

        public static bool SupportsSpecialProps(Drawable.Types.ObjectType type)
        {
            switch (type)
            {
                //case Drawable.Types.ObjectType.VerticalLine:
                //case Drawable.Types.ObjectType.HorizontalLine:
                case Drawable.Types.ObjectType.TrendLine: // RayMode, Angle
                case Drawable.Types.ObjectType.Rectangle: // Fill
                case Drawable.Types.ObjectType.Triangle: // Fill
                case Drawable.Types.ObjectType.Ellipse: // Fill
                //case Drawable.Types.ObjectType.Symbol:
                case Drawable.Types.ObjectType.Text: // Angle, AnchorPosition
                //case Drawable.Types.ObjectType.Bitmap: // ?Angle?, ?AnchorPosition?
                //case Drawable.Types.ObjectType.Levels:
                //case Drawable.Types.ObjectType.Cycles:
                case Drawable.Types.ObjectType.LinRegChannel: // RayMode, Fill
                case Drawable.Types.ObjectType.StdDevChannel: // RayMode, Fill
                case Drawable.Types.ObjectType.EquidistantChannel: // RayMode, Fill
                case Drawable.Types.ObjectType.GannLine: // RayMode, Angle, Scale
                case Drawable.Types.ObjectType.GannFan: // Scale, GannDirection
                case Drawable.Types.ObjectType.GannGrid: // Scale, GannDirection
                //case Drawable.Types.ObjectType.FiboFan:
                case Drawable.Types.ObjectType.FiboArcs: // Scale, FullEllipse
                case Drawable.Types.ObjectType.FiboChannel: // RayMode
                case Drawable.Types.ObjectType.FiboRetracement: // RayMode
                //case Drawable.Types.ObjectType.FiboTimeZones:
                case Drawable.Types.ObjectType.FiboExpansion: // RayMode
                case Drawable.Types.ObjectType.AndrewsPitchfork: // RayMode
                case Drawable.Types.ObjectType.LabelControl: // Angle, AnchorPosition
                //case Drawable.Types.ObjectType.RectangleControl:
                //case Drawable.Types.ObjectType.EditControl:
                //case Drawable.Types.ObjectType.ButtonControl:
                case Drawable.Types.ObjectType.BitmapControl: // AnchorPosition, ?Angle?
                //case Drawable.Types.ObjectType.TextBlockControl:
                    return true;
                default: return false;
            }
        }
    }
}
