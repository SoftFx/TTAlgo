using System;

namespace TickTrader.Algo.Api
{
    public static class DrawableCollectionExtensions
    {
        public static IDrawableObject VerticalLine(this IDrawableCollection collection, string name, DateTime time,
            OutputTargets targetWindow = OutputTargets.Overlay)
        {
            return collection.Create(name, DrawableObjectType.VerticalLine, targetWindow)
                .SetAnchorList(time);
        }

        public static IDrawableObject HorizontalLine(this IDrawableCollection collection, string name, double price,
            OutputTargets targetWindow = OutputTargets.Overlay)
        {
            return collection.Create(name, DrawableObjectType.HorizontalLine, targetWindow)
                .SetAnchorList(price);
        }

        public static IDrawableObject TrendLine(this IDrawableCollection collection, string name, DateTime time0, double price0,
            DateTime time1, double price1, OutputTargets targetWindow = OutputTargets.Overlay)
        {
            return collection.Create(name, DrawableObjectType.TrendLine, targetWindow)
                .SetAnchorList(time0, price0, time1, price1);
        }

        public static IDrawableObject TrendLineByAngle(this IDrawableCollection collection, string name, DateTime time, double price,
            double angle, OutputTargets targetWindow = OutputTargets.Overlay)
        {
            return collection.Create(name, DrawableObjectType.TrendLine, targetWindow)
                .SetAnchorList(time, price)
                .SetAngle(angle);
        }

        public static IDrawableObject Rectangle(this IDrawableCollection collection, string name, DateTime time0, double price0,
            DateTime time1, double price1, OutputTargets targetWindow = OutputTargets.Overlay)
        {
            return collection.Create(name, DrawableObjectType.Rectangle, targetWindow)
                .SetAnchorList(time0, price0, time1, price1);
        }

        public static IDrawableObject Triangle(this IDrawableCollection collection, string name, DateTime time0, double price0,
            DateTime time1, double price1, DateTime time2, double price2, OutputTargets targetWindow = OutputTargets.Overlay)
        {
            return collection.Create(name, DrawableObjectType.Triangle, targetWindow)
                .SetAnchorList(time0, price0, time1, price1, time2, price2);
        }

        public static IDrawableObject Ellipse(this IDrawableCollection collection, string name, DateTime time0, double price0,
            DateTime time1, double price1, double scale, OutputTargets targetWindow = OutputTargets.Overlay)
        {
            return collection.Create(name, DrawableObjectType.Ellipse, targetWindow)
                .SetAnchorList(time0, price0, time1, price1)
                .SetScale(scale);
        }

        public static IDrawableObject Symbol(this IDrawableCollection collection, string name, DateTime time, double price,
            OutputTargets targetWindow = OutputTargets.Overlay)
        {
            return collection.Create(name, DrawableObjectType.Symbol, targetWindow)
                .SetAnchorList(time, price);
        }

        public static IDrawableObject Text(this IDrawableCollection collection, string name, DateTime time, double price,
            OutputTargets targetWindow = OutputTargets.Overlay)
        {
            return collection.Create(name, DrawableObjectType.Text, targetWindow)
                .SetAnchorList(time, price);
        }

        public static IDrawableObject Bitmap(this IDrawableCollection collection, string name, DateTime time, double price,
            OutputTargets targetWindow = OutputTargets.Overlay)
        {
            return collection.Create(name, DrawableObjectType.Bitmap, targetWindow)
                .SetAnchorList(time, price);
        }

        public static IDrawableObject Cycles(this IDrawableCollection collection, string name, DateTime time0, double price0,
            DateTime time1, double price1, OutputTargets targetWindow = OutputTargets.Overlay)
        {
            return collection.Create(name, DrawableObjectType.Cycles, targetWindow)
                .SetAnchorList(time0, price0, time1, price1);
        }

        public static IDrawableObject LinRegChannel(this IDrawableCollection collection, string name, DateTime time0, DateTime time1,
            OutputTargets targetWindow = OutputTargets.Overlay)
        {
            return collection.Create(name, DrawableObjectType.LinRegChannel, targetWindow)
                .SetAnchorList(time0, 0, time1, 0);
        }

        public static IDrawableObject StdDevChannel(this IDrawableCollection collection, string name, DateTime time0, DateTime time1,
            double deviation, OutputTargets targetWindow = OutputTargets.Overlay)
        {
            return collection.Create(name, DrawableObjectType.StdDevChannel, targetWindow)
                .SetAnchorList(time0, 0, time1, 0)
                .SetDeviation(deviation);
        }

        public static IDrawableObject EquidistantChannel(this IDrawableCollection collection, string name, DateTime time0, double price0,
            DateTime time1, double price1, DateTime time2, double price2, OutputTargets targetWindow = OutputTargets.Overlay)
        {
            return collection.Create(name, DrawableObjectType.EquidistantChannel, targetWindow)
                .SetAnchorList(time0, price0, time1, price1, time2, price2);
        }

        public static IDrawableObject GannLine(this IDrawableCollection collection, string name, DateTime time0, double price0,
            DateTime time1, double angle, double scale, OutputTargets targetWindow = OutputTargets.Overlay)
        {
            return collection.Create(name, DrawableObjectType.GannLine, targetWindow)
                .SetAnchorList(time0, price0, time1, 0)
                .SetAngle(angle)
                .SetScale(scale);
        }

        public static IDrawableObject GannFan(this IDrawableCollection collection, string name, DateTime time0, double price0,
            DateTime time1, double scale, DrawableGannDirection direction, OutputTargets targetWindow = OutputTargets.Overlay)
        {
            return collection.Create(name, DrawableObjectType.GannFan, targetWindow)
                .SetAnchorList(time0, price0, time1, 0)
                .SetScale(scale)
                .SetGannDirection(direction);
        }

        public static IDrawableObject GannGrid(this IDrawableCollection collection, string name, DateTime time0, double price0,
            DateTime time1, double scale, DrawableGannDirection direction, OutputTargets targetWindow = OutputTargets.Overlay)
        {
            return collection.Create(name, DrawableObjectType.GannGrid, targetWindow)
                .SetAnchorList(time0, price0, time1, 0)
                .SetScale(scale)
                .SetGannDirection(direction);
        }

        public static IDrawableObject FiboFan(this IDrawableCollection collection, string name, DateTime time0, double price0,
            DateTime time1, double price1, OutputTargets targetWindow = OutputTargets.Overlay)
        {
            return collection.Create(name, DrawableObjectType.FiboFan, targetWindow)
                .SetAnchorList(time0, price0, time1, price1);
        }

        public static IDrawableObject FiboArcs(this IDrawableCollection collection, string name, DateTime time0, double price0,
            DateTime time1, double price1, double scale, bool fullEllipse, OutputTargets targetWindow = OutputTargets.Overlay)
        {
            return collection.Create(name, DrawableObjectType.FiboArcs, targetWindow)
                .SetAnchorList(time0, price0, time1, price1)
                .SetScale(scale)
                .SetFiboArcsFullEllipse(fullEllipse);
        }

        public static IDrawableObject FiboChannel(this IDrawableCollection collection, string name, DateTime time0, double price0,
            DateTime time1, double price1, DateTime time2, double price2, OutputTargets targetWindow = OutputTargets.Overlay)
        {
            return collection.Create(name, DrawableObjectType.FiboChannel, targetWindow)
                .SetAnchorList(time0, price0, time1, price1, time2, price2);
        }

        public static IDrawableObject FiboRetracement(this IDrawableCollection collection, string name, DateTime time0, double price0,
            DateTime time1, double price1, OutputTargets targetWindow = OutputTargets.Overlay)
        {
            return collection.Create(name, DrawableObjectType.FiboRetracement, targetWindow)
                .SetAnchorList(time0, price0, time1, price1);
        }

        public static IDrawableObject FiboTimeZones(this IDrawableCollection collection, string name, DateTime time0, double price0,
            DateTime time1, double price1, OutputTargets targetWindow = OutputTargets.Overlay)
        {
            return collection.Create(name, DrawableObjectType.FiboTimeZones, targetWindow)
                .SetAnchorList(time0, price0, time1, price1);
        }

        public static IDrawableObject FiboExpansion(this IDrawableCollection collection, string name, DateTime time0, double price0,
            DateTime time1, double price1, DateTime time2, double price2, OutputTargets targetWindow = OutputTargets.Overlay)
        {
            return collection.Create(name, DrawableObjectType.FiboExpansion, targetWindow)
                .SetAnchorList(time0, price0, time1, price1, time2, price2);
        }

        public static IDrawableObject AndrewsPitchfork(this IDrawableCollection collection, string name, DateTime time0, double price0,
            DateTime time1, double price1, DateTime time2, double price2, OutputTargets targetWindow = OutputTargets.Overlay)
        {
            return collection.Create(name, DrawableObjectType.AndrewsPitchfork, targetWindow)
                .SetAnchorList(time0, price0, time1, price1, time2, price2);
        }
    }
}
