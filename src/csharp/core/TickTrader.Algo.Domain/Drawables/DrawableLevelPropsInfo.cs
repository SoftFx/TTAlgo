namespace TickTrader.Algo.Domain
{
    public partial class DrawableLevelPropsInfo
    {
        // expected defaults are the same as real
        //partial void OnConstruction()
        //{
        //    Value = 0.0;
        //    Text = null;
        //    ColorArgb = null;
        //    LineThickness = null;
        //    LineStyle = Metadata.Types.LineStyle.UnknownLineStyle;
        //    FontFamily = null;
        //    FontSize = null;
        //}
    }


    public partial class DrawableObjectLevelsList
    {
        public int Count => Levels.Count;

        public DrawableLevelPropsInfo this[int index] => Levels[index];


        public DrawableObjectLevelsList(int cnt)
        {
            OnConstruction();

            for (var i = 0; i < cnt; i++)
                Levels.Add(new DrawableLevelPropsInfo());
        }


        partial void OnConstruction()
        {
            DefaultColorArgb = 0xff008000;
            DefaultLineThickness = 1;
            DefaultLineStyle = Metadata.Types.LineStyle.Solid;
            DefaultFontFamily = "Arial";
            DefaultFontSize = 8;
        }
    }
}
