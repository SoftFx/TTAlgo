using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    public class Optimizer : CrossDomainObject
    {
        private readonly Dictionary<string, ParamSeekSet> _params = new Dictionary<string, ParamSeekSet>();
        private ParamSeekStrategy _strategy;

        public void SetParameter(string paramId, ParamSeekSet seekSet)
        {
            _params[paramId] = seekSet;
        }

        public void SetStrategy(ParamSeekStrategy strategy)
        {
            _strategy = strategy;
        }

        public void Run()
        {

        }
    }
}
