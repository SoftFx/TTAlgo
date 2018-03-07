using System.Runtime.Serialization;
using System.Windows.Media;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model.Setup;

namespace TickTrader.Algo.Common.Model.Config.Version1
{
    [DataContract(Name = "Output", Namespace = "")]
    public abstract class Output : Property
    {
    }


    [DataContract(Name = "OutputColor", Namespace = "")]
    public class OutputColor
    {
        [DataMember]
        public float Alpha { get; set; }

        [DataMember]
        public float Red { get; set; }

        [DataMember]
        public float Green { get; set; }

        [DataMember]
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


    [DataContract(Name = "ColoredLineOutput", Namespace = "")]
    public class ColoredLineOutput : Output
    {
        [DataMember]
        public OutputColor LineColor { get; set; }

        [DataMember]
        public int LineThickness { get; set; }

        [DataMember]
        public LineStyles LineStyle { get; set; }

        [DataMember]
        public bool IsEnabled { get; set; }
    }


    [DataContract(Name = "MarkerSeriesOutput", Namespace = "")]
    public class MarkerSeriesOutput : Output
    {
        [DataMember]
        public OutputColor LineColor { get; set; }

        [DataMember]
        public int LineThickness { get; set; }

        [DataMember]
        public MarkerSizes MarkerSize { get; set; }

        [DataMember]
        public bool IsEnabled { get; set; }
    }
}
