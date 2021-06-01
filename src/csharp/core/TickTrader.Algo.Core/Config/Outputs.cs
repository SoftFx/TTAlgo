using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace TickTrader.Algo.Core.Config
{
    [DataContract(Name = "Output", Namespace = "TTAlgo.Config.v2")]
    public abstract class Output : Property
    {
        [DataMember(Name = "Enabled")]
        public bool IsEnabled { get; set; }

        [DataMember(Name = "Color")]
        public OutputColor LineColor { get; set; }

        [DataMember(Name = "Thickness")]
        public int LineThickness { get; set; }
    }


    [DataContract(Name = "OutputColor", Namespace = "TTAlgo.Config.v2")]
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


        public OutputColor Clone()
        {
            return new OutputColor { Alpha = Alpha, Red = Red, Green = Green, Blue = Blue };
        }

        public uint ToArgb()
        {
            Span<uint> intSpan = stackalloc uint[] { 0 };
            var byteSpan = MemoryMarshal.Cast<uint, byte>(intSpan);

            byteSpan[3] = (byte)((Math.Min(1.0f, Math.Max(0.0f, Alpha)) * 255.0f) + 0.5f);
            byteSpan[2] = ScRgbTosRgb(Red);
            byteSpan[1] = ScRgbTosRgb(Green);
            byteSpan[0] = ScRgbTosRgb(Blue);

            return intSpan[0];
        }

        public static OutputColor FromArgb(uint colorArgb)
        {
            Span<uint> intSpan = stackalloc uint[] { colorArgb };
            var byteSpan = MemoryMarshal.Cast<uint, byte>(intSpan);

            return new OutputColor
            {
                Alpha = ((float)byteSpan[3]) / 255.0f,
                Red = sRgbToScRgb(byteSpan[2]),
                Green = sRgbToScRgb(byteSpan[1]),
                Blue = sRgbToScRgb(byteSpan[0]),
            };
        }


        // from System.Windows.Media.Color sources
        private static float sRgbToScRgb(byte bval)
        {
            float val = ((float)bval / 255.0f);

            if (!(val > 0.0))       // Handles NaN case too. (Though, NaN isn't actually 
                                    // possible in this case.)
            {
                return (0.0f);
            }
            else if (val <= 0.04045)
            {
                return (val / 12.92f);
            }
            else if (val < 1.0f)
            {
                return (float)Math.Pow(((double)val + 0.055) / 1.055, 2.4);
            }
            else
            {
                return (1.0f);
            }
        }

        // from System.Windows.Media.Color sources
        private static byte ScRgbTosRgb(float val)
        {
            if (!(val > 0.0))       // Handles NaN case too
            {
                return (0);
            }
            else if (val <= 0.0031308)
            {
                return ((byte)((255.0f * val * 12.92f) + 0.5f));
            }
            else if (val < 1.0)
            {
                return ((byte)((255.0f * ((1.055f * (float)Math.Pow((double)val, (1.0 / 2.4))) - 0.055f)) + 0.5f));
            }
            else
            {
                return (255);
            }
        }
    }


    public enum LineStyles
    {
        Solid,
        Dots,
        DotsRare,
        DotsVeryRare,
        LinesDots,
        Lines
    }

    [DataContract(Name = "ColoredLineOutput", Namespace = "TTAlgo.Config.v2")]
    public class ColoredLineOutput : Output
    {
        [DataMember]
        public LineStyles LineStyle { get; set; }


        public override Property Clone()
        {
            return new ColoredLineOutput
            {
                Id = Id,
                IsEnabled = IsEnabled,
                LineStyle = LineStyle,
                LineThickness = LineThickness,
                LineColor = LineColor.Clone(),
            };
        }
    }


    public enum MarkerSizes
    {
        Large,
        Medium,
        Small,
    }

    [DataContract(Name = "MarkerSeriesOutput", Namespace = "TTAlgo.Config.v2")]
    public class MarkerSeriesOutput : Output
    {
        [DataMember]
        public MarkerSizes MarkerSize { get; set; }


        public override Property Clone()
        {
            return new MarkerSeriesOutput
            {
                Id = Id,
                IsEnabled = IsEnabled,
                MarkerSize = MarkerSize,
                LineThickness = LineThickness,
                LineColor = LineColor.Clone(),
            };
        }
    }
}
