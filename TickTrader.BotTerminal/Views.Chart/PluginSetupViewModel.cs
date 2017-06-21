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
using TickTrader.Algo.Common.Model.Setup;
using System.ComponentModel.DataAnnotations;

namespace TickTrader.BotTerminal
{
    internal class PluginSetupViewModel : Screen, IWindowModel
    {
        private Logger _logger;
        private bool _dlgResult;
        private PluginCatalog _catalog;
        private IAlgoSetupFactory _setupFactory;


        public PluginSetup Setup { get; private set; }

        public PluginCatalogItem PluginItem { get; private set; }

        public bool CanOk { get; private set; }

        public bool SetupCanBeSkipped => Setup.IsEmpty && Setup.Descriptor.IsValid;

        [Required(ErrorMessage = "Instance Id is required")]
        [StringLength(30, ErrorMessage = "Instance Id is too long")]
        public string InstanceId { get; set; }

        public bool Isolated { get; set; }

        public bool RunBot { get; set; }


        public event Action<PluginSetupViewModel, bool> Closed = delegate { };


        public PluginSetupViewModel(PluginCatalog catalog, PluginCatalogItem item, IAlgoSetupFactory setupFactory, PluginIdProvider idProvider)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            this.DisplayName = $"Settings - {item.DisplayName}";
            this.PluginItem = item;
            this._setupFactory = setupFactory;
            this._catalog = catalog;

            switch (item.Descriptor.AlgoLogicType)
            {
                case AlgoTypes.Indicator:
                    InstanceId = idProvider.GenerateIndicatorId(item.Descriptor.DisplayName);
                    break;
                case AlgoTypes.Robot:
                    InstanceId = idProvider.GenerateBotId(item.Descriptor.DisplayName);
                    break;
                default:
                    InstanceId = idProvider.GeneratePluginId(item.Descriptor.DisplayName);
                    break;
            }
            Isolated = false;
            RunBot = true;

            catalog.AllPlugins.Updated += AllPlugins_Updated;

            Init();
        }


        public void Reset()
        {
            Setup.Reset();
        }

        public void Ok()
        {
            _dlgResult = true;

            TryClose();
        }

        public void Cancel()
        {
            _dlgResult = false;
            TryClose();
        }

        public override void CanClose(Action<bool> callback)
        {
            callback(true);
            Closed(this, _dlgResult);
            Dispose();
        }

        private void Init()
        {
            if (Setup != null)
                Setup.ValidityChanged -= Validate;

            Setup = _setupFactory.CreateSetup(PluginItem.Ref);
            Setup.ValidityChanged += Validate;
            Validate();

            _logger.Debug("Init "
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
            _catalog.AllPlugins.Updated -= AllPlugins_Updated;
        }
    }
}
