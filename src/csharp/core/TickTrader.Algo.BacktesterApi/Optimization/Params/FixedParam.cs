namespace TickTrader.Algo.BacktesterApi
{
    public class FixedParam : ParamSeekSet
    {
        public FixedParam(object constVal)
        {
            Val = constVal;
        }

        public override int Size => 1;
        public object Val { get; }

        public override object GetParamValue(int valNo)
        {
            return Val;
        }
    }


    public class FixedParam<T> : ParamSeekSet<T>
    {
        public FixedParam(T constVal)
        {
            Val = constVal;
        }

        public override int Size => 1;
        public T Val { get; }

        protected override T GetValue(int valNo)
        {
            return Val;
        }
    }
}
