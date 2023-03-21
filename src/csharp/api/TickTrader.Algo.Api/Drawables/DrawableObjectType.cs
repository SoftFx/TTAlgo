namespace TickTrader.Algo.Api
{
    public enum DrawableObjectType
    {
        //Unknown = 0,
        VerticalLine = 1,
        HorizontalLine = 2,
        TrendLine = 3,
        Rectangle = 4,
        Triangle = 5,
        Ellipse = 6,
        Symbol = 7,
        Text = 8,
        Bitmap = 9,

        Levels = 30,
        Cycles = 31,
        LinRegChannel = 32,
        StdDevChannel = 33,
        EquidistantChannel = 34,

        GannLine = 40,
        GannFan = 41,
        GannGrid = 42,

        FiboFan = 50,
        FiboArcs = 51,
        FiboChannel = 52,
        FiboRetracement = 53,
        FiboTimeZones = 54,
        FiboExpansion = 55,
        AndrewsPitchfork = 56,

        LabelControl = 100,
        RectangleControl = 101,
        EditControl = 102,
        ButtonControl = 103,
        BitmapControl = 104,
        TextBlockControl = 105,
    }
}
