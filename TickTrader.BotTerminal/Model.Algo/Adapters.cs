using Caliburn.Micro;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.PointMarkers;
using SciChart.Charting.Visuals.RenderableSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Entities;
using TickTrader.Algo.GuiModel;

namespace TickTrader.BotTerminal
{
    //internal class DoubleSeriesAdapter
    //{
    //    private IIndicatorAdapterContext context;
    //    private OutputBuffer<double> buffer;
        
    //    private bool isBatchBuild;

    //    public DoubleSeriesAdapter(IIndicatorAdapterContext context, ColoredLineOutputSetup setup)
    //    {
    //        this.context = context;
    //        this.buffer = context.GetOutput<double>(setup.Descriptor.Id);
    //        this.SeriesData = new XyDataSeries<DateTime, double>();

    //        if (setup.IsEnabled)
    //        {
    //            buffer.Updated = Update;
    //            buffer.Appended = Append;

    //            buffer.BeginBatchBuild = () => isBatchBuild = true;

    //            buffer.EndBatchBuild = () =>
    //            {
    //                isBatchBuild = false;
    //                CopyAll();
    //            };

    //            SeriesData.SeriesName = setup.Descriptor.Id;
    //            FastLineRenderableSeries chartSeries = new FastLineRenderableSeries();
    //            chartSeries.DataSeries = SeriesData;
    //            chartSeries.Stroke = setup.LineColor;
    //            chartSeries.StrokeDashArray = setup.LineStyle.ToStrokeDashArray();
    //            chartSeries.StrokeThickness = setup.LineThickness;
    //            context.AddSeries(chartSeries);
    //        }
    //    }

    //    public XyDataSeries<DateTime, double> SeriesData { get; private set; }

    //    private void Append(int index, double data)
    //    {
    //        if (!isBatchBuild)
    //        {
    //            DateTime x = context.GetTimeCoordinate(index);
    //            Execute.OnUIThread(() => SeriesData.Append(x, data));
    //        }
    //    }

    //    private void Update(int index, double data)
    //    {
    //        if (!isBatchBuild)
    //        {
    //            DateTime x = context.GetTimeCoordinate(index);
    //            Execute.OnUIThread(() => SeriesData.YValues[index] = data);
    //        }
    //    }

    //    private void CopyAll()
    //    {
    //        Execute.OnUIThread(() =>
    //        {
    //            SeriesData.Clear();
    //            SeriesData.Append(EnumerateDateTimeCoordinate(), buffer);
    //        });
    //    }

    //    private IEnumerable<DateTime> EnumerateDateTimeCoordinate()
    //    {
    //        for (int i = 0; i < buffer.Count; i++)
    //            yield return context.GetTimeCoordinate(i);
    //    }
    //}

    //internal class MarkerSeriesAdapter
    //{
    //    private IIndicatorAdapterContext context;
    //    private OutputBuffer<Marker> buffer;
    //    private XyDataSeries<DateTime, double> seriesData = new XyDataSeries<DateTime, double>();

    //    private bool isBatchBuild;

    //    public MarkerSeriesAdapter(IIndicatorAdapterContext context, MarkerSeriesOutputSetup setup)
    //    {
    //        this.context = context;
    //        this.buffer = context.GetOutput<Marker>(setup.Descriptor.Id);

    //        if (setup.IsEnabled)
    //        {
    //            buffer.Updated = Update;
    //            buffer.Appended = Append;

    //            buffer.BeginBatchBuild = () => isBatchBuild = true;

    //            buffer.EndBatchBuild = () =>
    //            {
    //                isBatchBuild = false;
    //                CopyAll();
    //            };

    //            seriesData.SeriesName = setup.Descriptor.DisplayName;
    //            FastLineRenderableSeries chartSeries = new FastLineRenderableSeries();
    //            chartSeries.DataSeries = seriesData;
    //            chartSeries.StrokeThickness = 0;
    //            var markerTool = new AlgoPointMarker()
    //            {
    //                Stroke = setup.LineColor,
    //                StrokeThickness = setup.LineThickness
    //            };
    //            SetSize(markerTool, setup.MarkerSize);
    //            chartSeries.PointMarker = markerTool;
    //            context.AddSeries(chartSeries);
    //        }
    //    }

    //    private void SetSize(AlgoPointMarker markerTool, MarkerSizes size)
    //    {
    //        switch (size)
    //        {
    //            case MarkerSizes.Large: markerTool.Width = 16; markerTool.Height = 16; break;
    //            case MarkerSizes.Small: markerTool.Width = 4; markerTool.Height = 4; break;
    //            default: markerTool.Width = 8; markerTool.Height = 8; break;
    //        }
    //    }

    //    private void Append(int index, Marker marker)
    //    {
    //        if (!isBatchBuild)
    //        {
    //            DateTime x = context.GetTimeCoordinate(index);
    //            Execute.OnUIThread(() => seriesData.Append(x, marker.Y, new AlgoMarkerMetadata(marker)));
    //        }
    //    }

    //    private void Update(int index, Marker marker)
    //    {
    //        if (!isBatchBuild)
    //        {
    //            DateTime x = context.GetTimeCoordinate(index);
    //            Execute.OnUIThread(() =>
    //            {
    //                seriesData.Metadata[index] = new AlgoMarkerMetadata(marker);
    //                seriesData.YValues[index] = marker.Y;
    //            });
    //        }
    //    }

    //    private void CopyAll()
    //    {
    //        Execute.OnUIThread(() =>
    //        {
    //            for (int i = 0; i < buffer.Count; i++)
    //            {
    //                var marker = buffer[i];
    //                var x = context.GetTimeCoordinate(i);
    //                var y = marker.Y;
    //                seriesData.Append(x, y, new AlgoMarkerMetadata(marker));
    //            }
    //        });
    //    }

    //    private IEnumerable<DateTime> EnumerateDateTimeCoordinate()
    //    {
    //        for (int i = 0; i < buffer.Count; i++)
    //            yield return context.GetTimeCoordinate(i);
    //    }
    //}
}
