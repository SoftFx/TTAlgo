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
using TickTrader.Algo.Common.PluginSetup;

namespace TickTrader.BotTerminal
{
    internal class PluginSetupViewModel : Screen, IWindowModel
    {
        private Logger logger;
        //private IIndicatorSetup cfg;
        //private IIndicatorHost host;
        private bool dlgResult;
        private PluginCatalog catalog;
        private IAlgoSetupFactory setupFactory;

        public PluginSetupViewModel(PluginCatalog catalog, PluginCatalogItem item, IAlgoSetupFactory setupFactory)
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
            this.DisplayName = $"Settings - {item.DisplayName}";
            this.PluginItem = item;
            //this.host = host;
            this.setupFactory = setupFactory;
            this.catalog = catalog;

            catalog.AllPlugins.Updated += AllPlugins_Updated;

            Init();
        }

        public PluginSetup Setup { get; private set; }
        public PluginCatalogItem PluginItem { get; private set; }
        public bool SetupCanBeSkipped { get { return Setup.IsEmpty && Setup.Descriptor.IsValid; } }

        public void Reset()
        {
            Setup.Reset();
        }

        public bool CanOk { get; private set; }

        public event Action<PluginSetupViewModel, bool> Closed = delegate { };

        public void Ok()
        {
            dlgResult = true;

            //try
            //{
            //    host.AddOrUpdateIndicator(cfg);
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex);
            //}

            TryClose();
        }

        public void Cancel()
        {
            dlgResult = false;
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
            if (Setup != null)
                Setup.ValidityChanged -= Validate;

            Setup = setupFactory.CreateSetup(PluginItem.Ref);
            Setup.ValidityChanged += Validate;
            Validate();

            logger.Debug("Init "
                 + Setup.Parameters.Count() + " params "
                 + Setup.Inputs.Count() + " inputs "
                 + Setup.Outputs.Count() + " outputs ");

            NotifyOfPropertyChange(nameof(Setup));
        }

        private void Validate()
        {
            CanOk = Setup.IsValid;
            NotifyOfPropertyChange(nameof(CanOk));
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
