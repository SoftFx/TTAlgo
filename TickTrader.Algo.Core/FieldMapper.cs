using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    class FieldMapper<TSrc>
    {
        public Mapping MapReadonly<TField>(Func<TSrc, TField> getter)
        {
        }

        public Mapping Map<TField>(Func<TSrc, TField> getter, Action<TSrc, TField> setter)
        {
        }

        public class Mapping
        {
        }
    }
}
