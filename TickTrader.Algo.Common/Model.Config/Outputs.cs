using System.Runtime.Serialization;
using System.Windows.Media;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model.Setup;

namespace TickTrader.Algo.Common.Model.Config
{
    [DataContract(Name = "Output", Namespace = "TTAlgo.Setup.ver2")]
    public abstract class Output : Property
    {
        [DataMember(Name = "Enabled")]
        public bool IsEnabled { get; set; }

        [DataMember(Name = "Color")]
        public OutputColor LineColor { get; set; }

        [DataMember(Name = "Thickness")]
        public int LineThickness { get; set; }
    }


    [DataContract(Name = "OutputColor", Namespace = "TTAlgo.Setup.ver2")]
    public class OutputColor
    {
        [DataMember(Name = "Alpha")]
        public float Alpha { get; set; }

        [DataMember(Name = "Red")]
        public float Red { get; set; }

        [DataMember(Name = "Green")]
        public float Green { get; set; }

        [DataMember(Name = "Blue")]
        public float Blue { get; set; }


        public static OutputColor FromWindowsColor(Color color)
        {
            return new OutputColor
            {
                Alpha = color.ScA,
                Red = color.ScR,
                Green = color.ScG,
                Blue = color.ScB
            };
        }


        public Color ToWindowsColor()
        {
            return Color.FromScRgb(Alpha, Red, Green, Blue);
        }
    }


    [DataContract(Name = "ColoredLineOutput", Namespace = "TTAlgo.Setup.ver2")]
    public class ColoredLineOutput : Output
    {
        [DataMember]
        public LineStyles LineStyle { get; set; }
    }


    [DataContract(Name = "MarkerSeriesOutput", Namespace = "TTAlgo.Setup.ver2")]
    public class MarkerSeriesOutput : Output
    {
        [DataMember]
        public MarkerSizes MarkerSize { get; set; }
    }
}
