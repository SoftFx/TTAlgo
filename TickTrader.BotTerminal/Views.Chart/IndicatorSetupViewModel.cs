using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
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
        private Logger logger;
        private IIndicatorSetup cfg;
        private IIndicatorHost host;
        private bool dlgResult;
        private PluginCatalog catalog;

        public IndicatorSetupViewModel(PluginCatalog catalog, PluginCatalogItem item, IIndicatorHost host)
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
            this.DisplayName = "Add Indicator";
            this.PluginItem = item;
            this.host = host;
            this.catalog = catalog;

            catalog.AllPlugins.Updated += AllPlugins_Updated;

            Init();
        }

        public IndicatorSetupBase Setup { get { return cfg.UiModel; } }
        public PluginCatalogItem PluginItem { get; private set; }

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
                logger.Error(ex);
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

            cfg = host.CreateIndicatorConfig(PluginItem.Ref);
            cfg.UiModel.ValidityChanged += Validate;
            Validate();

           logger.Debug("Init "
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

        private void AllPlugins_Updated(Machinarium.Qnil.DictionaryUpdateArgs<PluginCatalogKey, PluginCatalogItem> args)
        {
            if (args.Action == DLinqAction.Replace)
            {
                if (args.Key == PluginItem.Key)
                {
                    PluginItem = args.NewItem;
                    Init();
                }
            }
            else if (args.Action == DLinqAction.Remove)
            {
                if (args.Key == PluginItem.Key)
                    TryClose();
            }
        }

        private void Dispose()
        {
            catalog.AllPlugins.Updated -= AllPlugins_Updated;
        }
    }
}
