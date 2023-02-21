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
            set => VisibilityBitmask = (int)value;
        }


        public DrawableObjectInfo(string name, Drawable.Types.ObjectType type)
        {
            Name = name;
            Type = type;
            CreatedTime = UtcTicks.Now;
            TargetWindow = Metadata.Types.OutputTarget.Overlay;
            VisibilityBitmask = (int)DrawableVisibility.AllTimeframes;

            InitFields();
        }


        private void InitFields()
        {
            switch (Type)
            {
                case Drawable.Types.ObjectType.VerticalLine:
                case Drawable.Types.ObjectType.HorizontalLine:
                    Anchors = new DrawableObjectAnchorsList(1);
                    LineProps = new DrawableLinePropsInfo();
                    break;
                case Drawable.Types.ObjectType.TrendLine:
                    Anchors = new DrawableObjectAnchorsList(2);
                    LineProps = new DrawableLinePropsInfo();
                    break;
                case Drawable.Types.ObjectType.Rectangle:
                    Anchors = new DrawableObjectAnchorsList(2);
                    ShapeProps = new DrawableShapePropsInfo();
                    break;
                case Drawable.Types.ObjectType.Triangle:
                case Drawable.Types.ObjectType.Ellipse:
                    Anchors = new DrawableObjectAnchorsList(3);
                    ShapeProps = new DrawableShapePropsInfo();
                    break;
                case Drawable.Types.ObjectType.Symbol:
                    Anchors = new DrawableObjectAnchorsList(1);
                    SymbolProps = new DrawableSymbolPropsInfo();
                    break;
                case Drawable.Types.ObjectType.Text:
                    Anchors = new DrawableObjectAnchorsList(1);
                    TextProps = new DrawableTextPropsInfo();
                    break;
                case Drawable.Types.ObjectType.Bitmap:
                    Anchors = new DrawableObjectAnchorsList(1);
                    BitmapProps = new DrawableBitmapPropsInfo();
                    break;
                case Drawable.Types.ObjectType.LabelControl:
                    TextProps = new DrawableTextPropsInfo();
                    ControlProps = new DrawableControlPropsInfo();
                    break;
                case Drawable.Types.ObjectType.RectangleControl:
                    ShapeProps = new DrawableShapePropsInfo();
                    ControlProps = new DrawableControlPropsInfo();
                    break;
                case Drawable.Types.ObjectType.EditControl:
                    TextProps = new DrawableTextPropsInfo();
                    ControlProps = new DrawableControlPropsInfo();
                    break;
                case Drawable.Types.ObjectType.ButtonControl:
                    TextProps = new DrawableTextPropsInfo();
                    ControlProps = new DrawableControlPropsInfo();
                    break;
                case Drawable.Types.ObjectType.BitmapControl:
                    BitmapProps = new DrawableBitmapPropsInfo();
                    ControlProps = new DrawableControlPropsInfo();
                    break;
                case Drawable.Types.ObjectType.Levels:
                    Levels = new DrawableObjectLevelsList(0);
                    break;
                case Drawable.Types.ObjectType.Cycles:
                    LineProps = new DrawableLinePropsInfo();
                    Anchors = new DrawableObjectAnchorsList(2);
                    break;
                case Drawable.Types.ObjectType.LinRegChannel:
                    LineProps = new DrawableLinePropsInfo();
                    Anchors = new DrawableObjectAnchorsList(2);
                    break;
                case Drawable.Types.ObjectType.StdDevChannel:
                    LineProps = new DrawableLinePropsInfo();
                    Anchors = new DrawableObjectAnchorsList(2);
                    break;
                case Drawable.Types.ObjectType.EquidistantChannel:
                    LineProps = new DrawableLinePropsInfo();
                    Anchors = new DrawableObjectAnchorsList(3);
                    break;
                case Drawable.Types.ObjectType.GannLine:
                    LineProps = new DrawableLinePropsInfo();
                    Anchors = new DrawableObjectAnchorsList(2);
                    break;
                case Drawable.Types.ObjectType.GannFan:
                    LineProps = new DrawableLinePropsInfo();
                    Anchors = new DrawableObjectAnchorsList(2);
                    break;
                case Drawable.Types.ObjectType.GannGrid:
                    LineProps = new DrawableLinePropsInfo();
                    Anchors = new DrawableObjectAnchorsList(2);
                    break;
                case Drawable.Types.ObjectType.FiboFan:
                    LineProps = new DrawableLinePropsInfo();
                    Anchors = new DrawableObjectAnchorsList(2);
                    Levels = new DrawableObjectLevelsList(0);
                    break;
                case Drawable.Types.ObjectType.FiboArcs:
                    LineProps = new DrawableLinePropsInfo();
                    Anchors = new DrawableObjectAnchorsList(2);
                    Levels = new DrawableObjectLevelsList(0);
                    break;
                case Drawable.Types.ObjectType.FiboChannel:
                    LineProps = new DrawableLinePropsInfo();
                    Anchors = new DrawableObjectAnchorsList(3);
                    Levels = new DrawableObjectLevelsList(0);
                    break;
                case Drawable.Types.ObjectType.FiboRetracement:
                    Anchors = new DrawableObjectAnchorsList(2);
                    Levels = new DrawableObjectLevelsList(0);
                    break;
                case Drawable.Types.ObjectType.FiboTimeZones:
                    Anchors = new DrawableObjectAnchorsList(2);
                    Levels = new DrawableObjectLevelsList(0);
                    break;
                case Drawable.Types.ObjectType.FiboExpansion:
                    Anchors = new DrawableObjectAnchorsList(3);
                    Levels = new DrawableObjectLevelsList(0);
                    break;
                case Drawable.Types.ObjectType.AndrewsPitchfork:
                    LineProps = new DrawableLinePropsInfo();
                    Anchors = new DrawableObjectAnchorsList(3);
                    Levels = new DrawableObjectLevelsList(0);
                    break;
                default: throw new AlgoException("Unsupported object type");
            }
        }
    }
}
