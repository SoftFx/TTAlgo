using Caliburn.Micro;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    internal class OptimizationResultsPageViewModel : Screen
    {
        private readonly BoolProperty _isVisibleProp = new BoolProperty();
        private Dictionary<string, DataColumn> _idToColumnMap = new Dictionary<string, DataColumn>();
        private DataColumn _metricColumn;

        public OptimizationResultsPageViewModel()
        {
        }

        public void Start(PluginDescriptor descriptor, Optimizer optimizer)
        {
            _isVisibleProp.Set();
            Clear();

            if (descriptor != null)
            {
                foreach (var par in descriptor.Parameters)
                {
                    var dataColumn = new DataColumn(par.DisplayName);
                    _idToColumnMap.Add(par.Id, dataColumn);
                    Data.Columns.Add(dataColumn);
                }
            }

            _metricColumn = new DataColumn("Metric", typeof(double));
            Data.Columns.Add(_metricColumn);
        }

        public void Stop(Optimizer optimizer)
        {
        }

        public void Update(OptCaseReport report)
        {
            var row = Data.NewRow();

            foreach (var pair in report.Config)
            {
                var col = _idToColumnMap[pair.Key];
                row[col] = pair.Value;
            }

            row[_metricColumn] = report.MetricVal;

            Data.Rows.Add(row);
        }

        public void Hide()
        {
            _isVisibleProp.Clear();
            Clear();
        }

        private void Clear()
        {
            Data.Clear();
            Data.Columns.Clear();
            _idToColumnMap.Clear();
        }

        public DataTable Data { get; } = new DataTable();
        public BoolVar IsVisible => _isVisibleProp.Var;
    }
}
