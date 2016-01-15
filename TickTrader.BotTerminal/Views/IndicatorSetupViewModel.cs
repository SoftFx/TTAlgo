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
        private IndicatorSetup setup;
        private IIndicatorSetupFactory setupFactory;
        private bool dlgResult;
        private AlgoRepositoryModel repModel;

        public IndicatorSetupViewModel(AlgoRepositoryModel repModel, AlgoRepositoryItem item, IIndicatorSetupFactory setupFactory)
        {
            this.DisplayName = "Add Indicator";
            this.RepItem = item;
            this.setupFactory = setupFactory;
            this.repModel = repModel;

            repModel.Removed += repModel_Removed;
            repModel.Replaced += repModel_Replaced;

            Init();
        }

        public List<PropertySetupBase> Properties { get; private set; }
        public IndicatorSetup Setup { get { return setup; } }
        public AlgoRepositoryItem RepItem { get; private set; }

        public void Reset()
        {
            setup.Reset();
        }

        public bool CanOk { get; private set; }

        public event Action<IndicatorSetupViewModel, bool> Closed = delegate { };

        public void Ok()
        {
            dlgResult = true;
            TryClose();
        }

        public override void CanClose(Action<bool> callback)
        {
            callback(true);
            Closed(this, dlgResult);
            Dispose();
        }

        private void Init()
        {
            IndicatorSetup oldSetup = setup;

            if (setup != null)
                setup.ValidityChanged -= Validate;

            setup = setupFactory.CreateSetup(RepItem.Descriptor);
            setup.ValidityChanged += Validate;
            Validate();

            Properties = new List<PropertySetupBase>(setup.Parameters);
            NotifyOfPropertyChange("Properties");
        }

        private void Validate()
        {
            CanOk = setup.IsValid;
            NotifyOfPropertyChange("CanOk");
        }

        void repModel_Replaced(AlgoRepositoryItem newItem)
        {
            if (newItem.Id == RepItem.Id)
            {
                RepItem = newItem;
                Init();
            }
        }

        void repModel_Removed(AlgoRepositoryItem removedItem)
        {
            if (removedItem.Id == RepItem.Id)
                TryClose();
        }

        private void Dispose()
        {
            repModel.Removed -= repModel_Removed;
            repModel.Replaced -= repModel_Replaced;
        }
    }

    internal interface IIndicatorSetupFactory
    {
        IndicatorSetup CreateSetup(AlgoInfo descriptor);
    }
}
