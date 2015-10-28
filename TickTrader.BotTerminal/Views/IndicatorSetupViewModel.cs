using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.BotTerminal
{
    internal class IndicatorSetupViewModel : Screen
    {
        private IndicatorSetupModel setup;

        public IndicatorSetupViewModel(AlgoRepositoryItem item)
        {
            DisplayName = "Add Indicator";

            //Parameters = new BindableCollection<ParameterModel>();
            //Outputs = new BindableCollection<OutputModel>();

            setup = new IndicatorSetupModel(item);

            Properties = new BindableCollection<AlgoPropertySetup>();
            Properties.AddRange(setup.Parameters);
        }

        public BindableCollection<AlgoPropertySetup> Properties { get; private set; }
    }
}
