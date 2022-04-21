namespace TickTrader.Algo.Core.Lib
{
    public static class HashCode
    {
        public static int Combine(int h1, int h2)
        {
            return ((h1 << 5) + h1) ^ h2;
        }

        public static int Combine(int h1, int h2, int h3)
        {
            return Combine(Combine(h1, h2), h3);
        }

        public static int Combine(int h1, int h2, int h3, int h4)
        {
            return Combine(Combine(h1, h2), Combine(h3, h4));
        }

        public static int Combine(int h1, int h2, int h3, int h4, int h5)
        {
            return Combine(Combine(h1, h2, h3, h4), h5);
        }

        public static int GetComposite<T1, T2>(T1 prop1, T2 prop2)
        {
            return Combine(prop1.GetHashCode(), prop2.GetHashCode());
        }

        public static int GetComposite<T1, T2, T3>(T1 prop1, T2 prop2, T3 prop3)
        {
            return Combine(prop1.GetHashCode(), prop2.GetHashCode(), prop3.GetHashCode());
        }

        public static int GetComposite<T1, T2, T3, T4>(T1 prop1, T2 prop2, T3 prop3, T4 prop4)
        {
            return Combine(prop1.GetHashCode(), prop2.GetHashCode(), prop3.GetHashCode(), prop4.GetHashCode());
        }
    }
}
