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
        private IIndicatorSetup cfg;
        private IIndicatorHost host;
        private bool dlgResult;
        private AlgoRepositoryModel repModel;

        public IndicatorSetupViewModel(AlgoRepositoryModel repModel, AlgoCatalogItem item, IIndicatorHost host)
        {
            this.DisplayName = "Add Indicator";
            this.RepItem = item;
            this.host = host;
            this.repModel = repModel;

            repModel.Removed += repModel_Removed;
            repModel.Replaced += repModel_Replaced;

            Init();
        }

        public IndicatorSetupBase Setup { get { return cfg.UiModel; } }
        public AlgoCatalogItem RepItem { get; private set; }

        public void Reset()
        {
            Setup.Reset();
        }

        public bool CanOk { get; private set; }

        public event Action<IndicatorSetupViewModel, bool> Closed = delegate { };

        public void Ok()
        {
            dlgResult = true;

            try
            {
                host.AddOrUpdateIndicator(cfg);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

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
            IIndicatorSetup oldCfg = cfg;

            if (cfg != null)
                cfg.UiModel.ValidityChanged -= Validate;

            cfg = host.CreateIndicatorConfig(RepItem);
            cfg.UiModel.ValidityChanged += Validate;
            Validate();

            System.Diagnostics.Debug.WriteLine("IndicatorSetupViewModel.Init() "
                + cfg.UiModel.Parameters.Count() + " params "
                + cfg.UiModel.Inputs.Count() + " inputs "
                + cfg.UiModel.Outputs.Count() + " outputs ");

            NotifyOfPropertyChange("Setup");
        }

        private void Validate()
        {
            CanOk = cfg.UiModel.IsValid;
            NotifyOfPropertyChange("CanOk");
        }

        void repModel_Replaced(AlgoCatalogItem newItem)
        {
            if (newItem.Id == RepItem.Id)
            {
                RepItem = newItem;
                Init();
            }
        }

        void repModel_Removed(AlgoCatalogItem removedItem)
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
}
