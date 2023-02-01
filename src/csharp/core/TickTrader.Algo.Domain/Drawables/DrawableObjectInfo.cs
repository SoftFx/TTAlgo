namespace TickTrader.Algo.Domain
{
    public partial class DrawableObjectInfo
    {
        public UtcTicks CreatedTime
        {
            get => new UtcTicks(CreatedTimeRaw);
            set => CreatedTimeRaw = value.Value;
        }


        public DrawableObjectInfo(string name, Drawable.Types.ObjectType type)
        {
            Name = name;
            Type = type;
            CreatedTime = UtcTicks.Now;

            InitFields();
        }


        private void InitFields()
        {
            switch (Type)
            {
                case Drawable.Types.ObjectType.VerticalLine:
                case Drawable.Types.ObjectType.HorizontalLine:
                    Anchors = new DrawableObjectAnchorsInfo(1);
                    LineProps = new DrawableLinePropsInfo();
                    break;
                case Drawable.Types.ObjectType.TrendLine:
                    Anchors = new DrawableObjectAnchorsInfo(2);
                    LineProps = new DrawableLinePropsInfo();
                    break;
                case Drawable.Types.ObjectType.Rectangle:
                    Anchors = new DrawableObjectAnchorsInfo(2);
                    ShapeProps = new DrawableShapePropsInfo();
                    break;
                case Drawable.Types.ObjectType.Triangle:
                case Drawable.Types.ObjectType.Ellipse:
                    Anchors = new DrawableObjectAnchorsInfo(3);
                    ShapeProps = new DrawableShapePropsInfo();
                    break;
                case Drawable.Types.ObjectType.Symbol:
                    Anchors = new DrawableObjectAnchorsInfo(1);
                    SymbolProps = new DrawableSymbolPropsInfo();
                    break;
                case Drawable.Types.ObjectType.Text:
                    break;
                case Drawable.Types.ObjectType.Bitmap:
                    break;
                case Drawable.Types.ObjectType.LabelControl:
                    break;
                case Drawable.Types.ObjectType.RectangleControl:
                    break;
                case Drawable.Types.ObjectType.EditControl:
                    break;
                case Drawable.Types.ObjectType.ButtonControl:
                    break;
                case Drawable.Types.ObjectType.BitmapControl:
                    break;
                default: throw new AlgoException("Unsupported object type");
            }
        }
    }
}
