namespace TickTrader.Algo.Api
{
    public enum DrawableObjectType
    {
        Unknown = 0,
        VerticalLine = 1,
        HorizontalLine = 2,
        TrendLine = 3,
        Rectangle = 4,
        Triangle = 5,
        Ellipse = 6,
        Symbol = 7,
        Text = 8,
        Bitmap = 9,

        LabelControl = 100,
        RectangleControl = 101,
        EditControl = 102,
        ButtonControl = 103,
        BitmapControl = 104,
    }
}
