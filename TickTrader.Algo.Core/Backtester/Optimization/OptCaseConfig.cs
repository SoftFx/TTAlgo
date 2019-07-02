using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class OptCaseConfig : Dictionary<string, object>
    {

        //private List<Action<IPluginSetupTarget>> _setupActions = new List<Action<IPluginSetupTarget>>();

        //public void Add(Action<IPluginSetupTarget> setupAction)
        //{
        //    _setupActions.Add(setupAction);
        //}

        public void Apply(IPluginSetupTarget target)
        {
            foreach (var pair in this)
                target.SetParameter(pair.Key, pair.Value);
        }
    }
}
