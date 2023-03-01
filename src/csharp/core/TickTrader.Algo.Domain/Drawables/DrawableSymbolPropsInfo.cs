namespace TickTrader.Algo.Domain
{
    public partial class DrawableSymbolPropsInfo
    {
        partial void OnConstruction() // default .ctor is taken by protobuf
        {
            Code = 241;
            Size = 14;
            ColorArgb = 0xff008000;
            FontFamily = "Wingdings";
            Anchor = Drawable.Types.SymbolAnchor.Bottom;
        }
    }
}
