﻿using System.Collections.Generic;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    public class OutputSeriesProxy
    {
        private const int DefaultUpdateCount = 16;

        private readonly object _syncObj = new();

        private List<OutputSeriesUpdate> _pendingUpdates = new(DefaultUpdateCount);


        public string PluginId { get; init; }

        public OutputDescriptor Descriptor { get; set; }

        public IOutputConfig Config { get; set; }

        public string SeriesId => Descriptor.Id;


        public IEnumerable<OutputSeriesUpdate> TakePendingUpdates()
        {
            lock (_syncObj)
            {
                var res = _pendingUpdates;
                _pendingUpdates = new List<OutputSeriesUpdate>(DefaultUpdateCount);
                return res;
            }
        }

        internal void AddUpdate(OutputSeriesUpdate update)
        {
            //if (SeriesId != update.SeriesId)
            //    return;

            lock (_syncObj)
            {
                if (update.UpdateAction == DataSeriesUpdate.Types.Action.Reset)
                    _pendingUpdates.Clear();

                _pendingUpdates.Add(update);
            }
        }
    }
}