namespace TickTrader.Algo.Api
{
    public interface Marker
    {
        double Y { get; set; }
        MarkerIcons Icon { get; set; }
        string DisplayText { get; set; }
        Colors Color { get; set; }
        ushort IconCode { get; set; }

        void Clear();
    }

    public enum MarkerIcons
    {
        Circle = 0,
        UpArrow = 1,
        DownArrow = 2,
        UpTriangle = 3,
        DownTriangle = 4,
        Diamond = 5,
        Square = 6,
        Wingdings = 7,
    }
}
