using SciChart.Charting.ChartModifiers;
using SciChart.Charting.Visuals.Axes;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    public abstract class AxisModifierBase : ChartModifierBase
    {
        public override void OnAttached()
        {
            base.OnAttached();
            AttachProvider(ParentSurface.XAxes);
            ParentSurface.XAxes.CollectionChanged += XAxes_CollectionChanged;
        }

        public override void OnDetached()
        {
            base.OnDetached();
            DeattachProvider(ParentSurface.XAxes);
            ParentSurface.XAxes.CollectionChanged -= XAxes_CollectionChanged;
        }

        protected abstract void AttachTo(IAxis axis);
        protected abstract void DeattachFrom(IAxis axis);

        private void XAxes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
                DeattachProvider(e.OldItems.Cast<IAxis>());
            else if (e.Action == NotifyCollectionChangedAction.Add)
                AttachProvider(e.NewItems.Cast<IAxis>());
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                AttachProvider(e.NewItems.Cast<IAxis>());
                DeattachProvider(e.OldItems.Cast<IAxis>());
            }
        }

        private void AttachProvider(IEnumerable<IAxis> axes)
        {
            foreach (var axis in ParentSurface.XAxes)
                AttachTo(axis);
        }

        private void DeattachProvider(IEnumerable<IAxis> axes)
        {
            foreach (var axis in ParentSurface.XAxes)
                DeattachFrom(axis);
        }
    }
}
