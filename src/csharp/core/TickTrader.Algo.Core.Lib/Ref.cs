namespace TickTrader.Algo.Core.Lib
{
    public static class Ref
    {
        public static void Swap<T>(ref T ref1, ref T ref2)
        {
            var ref1Copy = ref1;
            ref1 = ref2;
            ref2 = ref1Copy;
        }
    }
}
