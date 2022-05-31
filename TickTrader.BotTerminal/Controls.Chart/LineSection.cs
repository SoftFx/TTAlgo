using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;

namespace TickTrader.BotTerminal.Controls.Chart
{
    internal class LineSection : RectangularSection
    {
        internal virtual double? X
        {
            get => Xi;

            set
            {
                if (Xi == value) 
                    return;

                Xi = value;
                Xj = value;
            }
        }

        internal virtual double? Y
        {
            get => Yi;

            set
            {
                if (Yi == value)
                    return;

                Yi = value;
                Yj = value;
            }
        }

        internal LineSection() : base() { }

        internal LineSection(SolidColorPaint stroke) : base()
        {
            Stroke = stroke;
        }
    }
}
