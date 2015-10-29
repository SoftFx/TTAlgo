using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.GuiModel;

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

            setup = new IndicatorSetupModel(item.Descriptor);
            setup.ValidityChanged += Validate;
            Validate();

            Properties = new List<AlgoProperty>(setup.Parameters);
        }

        public List<AlgoProperty> Properties { get; private set; }

        public void Reset()
        {
            setup.Reset();
        }

        public bool CanOk { get; private set; }

        public void Ok()
        {

        }

        private void Validate()
        {
            CanOk = setup.IsValid;
            NotifyOfPropertyChange("CanOk");
        }
    }
}
