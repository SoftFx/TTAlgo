namespace TickTrader.Algo.Backtester
{
    public class ConstParam : ParamSeekSet
    {
        public ConstParam(object constVal)
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


    public class ConstParam<T> : ParamSeekSet<T>
    {
        public ConstParam(T constVal)
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
