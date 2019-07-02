using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    public class BoolVar : Var<bool>
    {
        public BoolVar()
        {
        }

        public BoolVar(bool initialValue = false) : base(initialValue)
        {
        }

        // Do not implement implicit convertors! It's a huge source of possible errors!
        //public static implicit operator BoolVar(bool constVal)
        //{
        //    return new BoolVar(constVal);
        //}

        public static BoolVar operator &(BoolVar c1, BoolVar c2)
        {
            return Operator<BoolVar>(() => c1.Value && c2.Value, c1, c2);
        }

        public static BoolVar operator &(BoolVar c1, bool c2)
        {
            return Operator<BoolVar>(() => c1.Value && c2, c1);
        }

        public static BoolVar operator &(bool c1, BoolVar c2)
        {
            return Operator<BoolVar>(() => c1 && c2.Value, c2);
        }

        public static BoolVar operator |(BoolVar c1, BoolVar c2)
        {
            return Operator<BoolVar>(() => c1.Value || c2.Value, c1, c2);
        }

        public static BoolVar operator |(bool c1, BoolVar c2)
        {
            return Operator<BoolVar>(() => c1 || c2.Value, c2);
        }

        public static BoolVar operator |(BoolVar c1, bool c2)
        {
            return Operator<BoolVar>(() => c1.Value || c2, c1);
        }

        public static BoolVar operator !(BoolVar c1)
        {
            return Operator<BoolVar>(() => !c1.Value, c1);
        }

        internal override bool Equals(bool val1, bool val2)
        {
            return val1 == val2;
        }
    }
}
