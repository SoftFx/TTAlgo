using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.CoreV1
{
    public static class ApiEnumConverter
    {
        public static CommissionType ToApiEnum(this Domain.CommissonInfo.Types.ValueType type)
        {
            switch (type)
            {
                case Domain.CommissonInfo.Types.ValueType.Points: return CommissionType.PerUnit;
                case Domain.CommissonInfo.Types.ValueType.Percentage: return CommissionType.Percent;
                case Domain.CommissonInfo.Types.ValueType.Money: return CommissionType.Absolute;

                default: throw new ArgumentException($"Unsupported commission charge type: {type}");
            }
        }

        public static AccountTypes ToApiEnum(this Domain.AccountInfo.Types.Type type)
        {
            switch (type)
            {
                case Domain.AccountInfo.Types.Type.Gross: return AccountTypes.Gross;
                case Domain.AccountInfo.Types.Type.Net: return AccountTypes.Net;
                case Domain.AccountInfo.Types.Type.Cash: return AccountTypes.Cash;

                default: throw new ArgumentException($"Unsupported commission charge type: {type}");
            }
        }

        public static OrderSide ToApiEnum(this Domain.OrderInfo.Types.Side side)
        {
            switch (side)
            {
                case Domain.OrderInfo.Types.Side.Buy: return OrderSide.Buy;
                case Domain.OrderInfo.Types.Side.Sell: return OrderSide.Sell;

                default: throw new ArgumentException($"Unsupported order side {side}");
            }
        }

        public static Domain.OrderInfo.Types.Side ToDomainEnum(this OrderSide side)
        {
            switch (side)
            {
                case OrderSide.Buy: return Domain.OrderInfo.Types.Side.Buy;
                case OrderSide.Sell: return Domain.OrderInfo.Types.Side.Sell;

                default: throw new ArgumentException($"Unsupported type {side}");
            }
        }

        public static OrderType ToApiEnum(this Domain.OrderInfo.Types.Type type)
        {
            switch (type)
            {
                case Domain.OrderInfo.Types.Type.Limit: return OrderType.Limit;
                case Domain.OrderInfo.Types.Type.Market: return OrderType.Market;
                case Domain.OrderInfo.Types.Type.Stop: return OrderType.Stop;
                case Domain.OrderInfo.Types.Type.StopLimit: return OrderType.StopLimit;
                case Domain.OrderInfo.Types.Type.Position: return OrderType.Position;

                default: throw new ArgumentException($"Unsupported type {type}");
            }
        }

        public static Domain.OrderInfo.Types.Type ToDomainEnum(this OrderType type)
        {
            switch (type)
            {
                case OrderType.Limit: return Domain.OrderInfo.Types.Type.Limit;
                case OrderType.Market: return Domain.OrderInfo.Types.Type.Market;
                case OrderType.Stop: return Domain.OrderInfo.Types.Type.Stop;
                case OrderType.StopLimit: return Domain.OrderInfo.Types.Type.StopLimit;
                case OrderType.Position: return Domain.OrderInfo.Types.Type.Position;

                default: throw new ArgumentException($"Unsupported type {type}");
            }
        }

        public static SlippageType ToApiEnum(this Domain.SlippageInfo.Types.Type type)
        {
            switch (type)
            {
                case Domain.SlippageInfo.Types.Type.Percent: return SlippageType.Percent;
                case Domain.SlippageInfo.Types.Type.Pips: return SlippageType.Pips;

                default: throw new ArgumentException($"Unsupported type {type}");
            }
        }

        public static OrderOptions ToApiEnum(this Domain.OrderOptions options)
        {
            return (OrderOptions)options;
        }

        public static Domain.OrderExecOptions ToDomainEnum(this OrderExecOptions options)
        {
            return (Domain.OrderExecOptions)options;
        }

        public static Domain.OrderExecOptions ToDomainEnum(this OrderOptions options)
        {
            return (Domain.OrderExecOptions)options;
        }

        public static OrderCmdResultCodes ToApiEnum(this Domain.OrderExecReport.Types.CmdResultCode code)
        {
            switch (code)
            {
                case Domain.OrderExecReport.Types.CmdResultCode.Ok: return OrderCmdResultCodes.Ok;
                case Domain.OrderExecReport.Types.CmdResultCode.UnknownError: return OrderCmdResultCodes.UnknownError;
                case Domain.OrderExecReport.Types.CmdResultCode.InternalError: return OrderCmdResultCodes.InternalError;
                case Domain.OrderExecReport.Types.CmdResultCode.ConnectionError: return OrderCmdResultCodes.ConnectionError;
                case Domain.OrderExecReport.Types.CmdResultCode.Timeout: return OrderCmdResultCodes.Timeout;
                case Domain.OrderExecReport.Types.CmdResultCode.TradeServerError: return OrderCmdResultCodes.TradeServerError;
                case Domain.OrderExecReport.Types.CmdResultCode.DealerReject: return OrderCmdResultCodes.DealerReject;
                case Domain.OrderExecReport.Types.CmdResultCode.Unsupported: return OrderCmdResultCodes.Unsupported;
                case Domain.OrderExecReport.Types.CmdResultCode.SymbolNotFound: return OrderCmdResultCodes.SymbolNotFound;
                case Domain.OrderExecReport.Types.CmdResultCode.OrderNotFound: return OrderCmdResultCodes.OrderNotFound;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectVolume: return OrderCmdResultCodes.IncorrectVolume;
                case Domain.OrderExecReport.Types.CmdResultCode.OffQuotes: return OrderCmdResultCodes.OffQuotes;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectOrderId: return OrderCmdResultCodes.IncorrectOrderId;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectPrice: return OrderCmdResultCodes.IncorrectPrice;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectTp: return OrderCmdResultCodes.IncorrectTp;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectSl: return OrderCmdResultCodes.IncorrectSl;
                case Domain.OrderExecReport.Types.CmdResultCode.NotEnoughMoney: return OrderCmdResultCodes.NotEnoughMoney;
                case Domain.OrderExecReport.Types.CmdResultCode.TradeNotAllowed: return OrderCmdResultCodes.TradeNotAllowed;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectStopPrice: return OrderCmdResultCodes.IncorrectStopPrice;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectMaxVisibleVolume: return OrderCmdResultCodes.IncorrectMaxVisibleVolume;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectExpiration: return OrderCmdResultCodes.IncorrectExpiration;
                case Domain.OrderExecReport.Types.CmdResultCode.DealingTimeout: return OrderCmdResultCodes.DealingTimeout;
                case Domain.OrderExecReport.Types.CmdResultCode.Misconfiguration: return OrderCmdResultCodes.Misconfiguration;
                case Domain.OrderExecReport.Types.CmdResultCode.OrderLocked: return OrderCmdResultCodes.OrderLocked;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectPricePrecision: return OrderCmdResultCodes.IncorrectPricePrecision;
                case Domain.OrderExecReport.Types.CmdResultCode.CloseOnlyTrading: return OrderCmdResultCodes.CloseOnlyTrading;
                case Domain.OrderExecReport.Types.CmdResultCode.MarketWithMaxVisibleVolume: return OrderCmdResultCodes.MarketWithMaxVisibleVolume;
                case Domain.OrderExecReport.Types.CmdResultCode.InvalidAmountChange: return OrderCmdResultCodes.InvalidAmountChange;
                case Domain.OrderExecReport.Types.CmdResultCode.CannotBeModified: return OrderCmdResultCodes.CannotBeModified;
                case Domain.OrderExecReport.Types.CmdResultCode.MaxVisibleVolumeNotSupported: return OrderCmdResultCodes.MaxVisibleVolumeNotSupported;
                case Domain.OrderExecReport.Types.CmdResultCode.ReadOnlyAccount: return OrderCmdResultCodes.ReadOnlyAccount;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectSlippage: return OrderCmdResultCodes.IncorrectSlippage;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectType: return OrderCmdResultCodes.IncorrectType;
                case Domain.OrderExecReport.Types.CmdResultCode.ThrottlingError: return OrderCmdResultCodes.ThrottlingError;
                case Domain.OrderExecReport.Types.CmdResultCode.OcoRelatedIdNotFound: return OrderCmdResultCodes.OCORelatedIdNotFound;
                case Domain.OrderExecReport.Types.CmdResultCode.OcoRelatedOrderAlreadyExists: return OrderCmdResultCodes.OCORelatedOrderAlreadyExists;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectSymbol: return OrderCmdResultCodes.IncorrectSymbol;
                case Domain.OrderExecReport.Types.CmdResultCode.OcoAlreadyExists: return OrderCmdResultCodes.OCOAlreadyExists;
                case Domain.OrderExecReport.Types.CmdResultCode.ExceededOrderLimit: return OrderCmdResultCodes.ExceededOrderLimit;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectTriggerTime: return OrderCmdResultCodes.IncorrectTriggerTime;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectTriggerOrderId: return OrderCmdResultCodes.IncorrectTriggerOrderId;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectTriggerOrderType: return OrderCmdResultCodes.IncorrectTriggerOrderType;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectConditionsForTrigger: return OrderCmdResultCodes.IncorrectConditionsForTrigger;
                case Domain.OrderExecReport.Types.CmdResultCode.OcoRelatedOrderIncorrectOptions: return OrderCmdResultCodes.OCORelatedOrderIncorrectOptions;
                case Domain.OrderExecReport.Types.CmdResultCode.OcoIncorrectRelatedId: return OrderCmdResultCodes.OCOIncorrectRelatedId;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectSide: return OrderCmdResultCodes.IncorrectSide;
                default: throw new ArgumentException($"Unsupported code {code}");
            }
        }

        public static Domain.HistoryRequestOptions ToDomainEnum(this ThQueryOptions options)
        {
            var res = Domain.HistoryRequestOptions.NoOptions;

            if (options.HasFlag(ThQueryOptions.SkipCanceled))
                res |= Domain.HistoryRequestOptions.SkipCanceled;
            if (options.HasFlag(ThQueryOptions.Backwards))
                res |= Domain.HistoryRequestOptions.Backwards;
            if (options.HasFlag(ThQueryOptions.SkipFailed))
                res |= Domain.HistoryRequestOptions.SkipFailed;

            return res;
        }

        public static TradeExecActions ToApiEnum(this Domain.TradeReportInfo.Types.ReportType reportType)
        {
            switch (reportType)
            {
                case Domain.TradeReportInfo.Types.ReportType.NoType: return TradeExecActions.None;
                case Domain.TradeReportInfo.Types.ReportType.OrderOpened: return TradeExecActions.OrderOpened;
                case Domain.TradeReportInfo.Types.ReportType.OrderCanceled: return TradeExecActions.OrderCanceled;
                case Domain.TradeReportInfo.Types.ReportType.OrderExpired: return TradeExecActions.OrderExpired;
                case Domain.TradeReportInfo.Types.ReportType.OrderFilled: return TradeExecActions.OrderFilled;
                case Domain.TradeReportInfo.Types.ReportType.PositionClosed: return TradeExecActions.PositionClosed;
                case Domain.TradeReportInfo.Types.ReportType.PositionOpened: return TradeExecActions.PositionOpened;
                case Domain.TradeReportInfo.Types.ReportType.BalanceTransaction: return TradeExecActions.BalanceTransaction;
                case Domain.TradeReportInfo.Types.ReportType.Credit: return TradeExecActions.Credit;
                case Domain.TradeReportInfo.Types.ReportType.OrderActivated: return TradeExecActions.OrderActivated;
                case Domain.TradeReportInfo.Types.ReportType.TradeModified: return TradeExecActions.TradeModified;

                default: throw new ArgumentException($"Unsupported report type {reportType}");
            }
        }

        public static Domain.Feed.Types.MarketSide ToDomainEnum(this BarPriceType priceType)
        {
            switch (priceType)
            {
                case BarPriceType.Bid: return Domain.Feed.Types.MarketSide.Bid;
                case BarPriceType.Ask: return Domain.Feed.Types.MarketSide.Ask;

                default: throw new ArgumentException($"Unsupported bar price type {priceType}");
            }
        }

        public static Domain.Feed.Types.Timeframe ToDomainEnum(this TimeFrames timeframe)
        {
            switch (timeframe)
            {
                case TimeFrames.MN: return Domain.Feed.Types.Timeframe.MN;
                case TimeFrames.D: return Domain.Feed.Types.Timeframe.D;
                case TimeFrames.W: return Domain.Feed.Types.Timeframe.W;
                case TimeFrames.H4: return Domain.Feed.Types.Timeframe.H4;
                case TimeFrames.H1: return Domain.Feed.Types.Timeframe.H1;
                case TimeFrames.M30: return Domain.Feed.Types.Timeframe.M30;
                case TimeFrames.M15: return Domain.Feed.Types.Timeframe.M15;
                case TimeFrames.M5: return Domain.Feed.Types.Timeframe.M5;
                case TimeFrames.M1: return Domain.Feed.Types.Timeframe.M1;
                case TimeFrames.S10: return Domain.Feed.Types.Timeframe.S10;
                case TimeFrames.S1: return Domain.Feed.Types.Timeframe.S1;
                case TimeFrames.Ticks: return Domain.Feed.Types.Timeframe.Ticks;
                case TimeFrames.TicksLevel2: return Domain.Feed.Types.Timeframe.TicksLevel2;

                default: throw new ArgumentException($"Unsupported timeframe {timeframe}");
            }
        }

        public static TimeFrames ToApiEnum(this Domain.Feed.Types.Timeframe timeframe)
        {
            switch (timeframe)
            {
                case Domain.Feed.Types.Timeframe.MN: return TimeFrames.MN;
                case Domain.Feed.Types.Timeframe.D: return TimeFrames.D;
                case Domain.Feed.Types.Timeframe.W: return TimeFrames.W;
                case Domain.Feed.Types.Timeframe.H4: return TimeFrames.H4;
                case Domain.Feed.Types.Timeframe.H1: return TimeFrames.H1;
                case Domain.Feed.Types.Timeframe.M30: return TimeFrames.M30;
                case Domain.Feed.Types.Timeframe.M15: return TimeFrames.M15;
                case Domain.Feed.Types.Timeframe.M5: return TimeFrames.M5;
                case Domain.Feed.Types.Timeframe.M1: return TimeFrames.M1;
                case Domain.Feed.Types.Timeframe.S10: return TimeFrames.S10;
                case Domain.Feed.Types.Timeframe.S1: return TimeFrames.S1;
                case Domain.Feed.Types.Timeframe.Ticks: return TimeFrames.Ticks;
                case Domain.Feed.Types.Timeframe.TicksLevel2: return TimeFrames.TicksLevel2;

                default: throw new ArgumentException($"Unsupported timeframe {timeframe}");
            }
        }

        public static MarkerIcons ToApiEnum(this Domain.MarkerInfo.Types.IconType icon)
        {
            switch (icon)
            {
                case Domain.MarkerInfo.Types.IconType.Circle: return MarkerIcons.Circle;
                case Domain.MarkerInfo.Types.IconType.UpArrow: return MarkerIcons.UpArrow;
                case Domain.MarkerInfo.Types.IconType.DownArrow: return MarkerIcons.DownArrow;
                case Domain.MarkerInfo.Types.IconType.UpTriangle: return MarkerIcons.UpTriangle;
                case Domain.MarkerInfo.Types.IconType.DownTriangle: return MarkerIcons.DownTriangle;
                case Domain.MarkerInfo.Types.IconType.Diamond: return MarkerIcons.Diamond;
                case Domain.MarkerInfo.Types.IconType.Square: return MarkerIcons.Square;

                default: throw new ArgumentException($"Unsupported icon {icon}");
            }
        }

        public static Domain.MarkerInfo.Types.IconType ToDomainEnum(this MarkerIcons icon)
        {
            switch (icon)
            {
                case MarkerIcons.Circle: return Domain.MarkerInfo.Types.IconType.Circle;
                case MarkerIcons.UpArrow: return Domain.MarkerInfo.Types.IconType.UpArrow;
                case MarkerIcons.DownArrow: return Domain.MarkerInfo.Types.IconType.DownArrow;
                case MarkerIcons.UpTriangle: return Domain.MarkerInfo.Types.IconType.UpTriangle;
                case MarkerIcons.DownTriangle: return Domain.MarkerInfo.Types.IconType.DownTriangle;
                case MarkerIcons.Diamond: return Domain.MarkerInfo.Types.IconType.Diamond;
                case MarkerIcons.Square: return Domain.MarkerInfo.Types.IconType.Square;

                default: throw new ArgumentException($"Unsupported icon {icon}");
            }
        }

        public static Domain.Metadata.Types.LineStyle ToDomainEnum(this LineStyles line)
        {
            switch (line)
            {
                case LineStyles.Solid: return Domain.Metadata.Types.LineStyle.Solid;
                case LineStyles.Dots: return Domain.Metadata.Types.LineStyle.Dots;
                case LineStyles.DotsRare: return Domain.Metadata.Types.LineStyle.DotsRare;
                case LineStyles.DotsVeryRare: return Domain.Metadata.Types.LineStyle.DotsVeryRare;
                case LineStyles.LinesDots: return Domain.Metadata.Types.LineStyle.LinesDots;
                case LineStyles.Lines: return Domain.Metadata.Types.LineStyle.Lines;

                default: throw new ArgumentException($"Unsupported line style {line}");
            }
        }

        public static LineStyles ToApiEnum(this Domain.Metadata.Types.LineStyle line)
        {
            switch (line)
            {
                case Domain.Metadata.Types.LineStyle.Solid: return LineStyles.Solid;
                case Domain.Metadata.Types.LineStyle.Dots: return LineStyles.Dots;
                case Domain.Metadata.Types.LineStyle.DotsRare: return LineStyles.DotsRare;
                case Domain.Metadata.Types.LineStyle.DotsVeryRare: return LineStyles.DotsVeryRare;
                case Domain.Metadata.Types.LineStyle.LinesDots: return LineStyles.LinesDots;
                case Domain.Metadata.Types.LineStyle.Lines: return LineStyles.Lines;

                default: throw new ArgumentException($"Unsupported line style {line}");
            }
        }

        public static Domain.Metadata.Types.PlotType ToDomainEnum(this PlotType type)
        {
            switch (type)
            {
                case PlotType.Line: return Domain.Metadata.Types.PlotType.Line;
                case PlotType.DiscontinuousLine: return Domain.Metadata.Types.PlotType.DiscontinuousLine;
                case PlotType.Points: return Domain.Metadata.Types.PlotType.Points;
                case PlotType.Histogram: return Domain.Metadata.Types.PlotType.Histogram;

                default: throw new ArgumentException($"Unsupported plot type {type}");
            }
        }

        public static Domain.Metadata.Types.OutputTarget ToDomainEnum(this OutputTargets target)
        {
            switch (target)
            {
                case OutputTargets.Overlay: return Domain.Metadata.Types.OutputTarget.Overlay;
                case OutputTargets.Window1: return Domain.Metadata.Types.OutputTarget.Window1;
                case OutputTargets.Window2: return Domain.Metadata.Types.OutputTarget.Window2;
                case OutputTargets.Window3: return Domain.Metadata.Types.OutputTarget.Window3;
                case OutputTargets.Window4: return Domain.Metadata.Types.OutputTarget.Window4;

                default: throw new ArgumentException($"Unsupported output target {target}");
            }
        }

        public static OutputTargets ToApiEnum(this Domain.Metadata.Types.OutputTarget target)
        {
            switch (target)
            {
                case Domain.Metadata.Types.OutputTarget.Overlay: return OutputTargets.Overlay;
                case Domain.Metadata.Types.OutputTarget.Window1: return OutputTargets.Window1;
                case Domain.Metadata.Types.OutputTarget.Window2: return OutputTargets.Window2;
                case Domain.Metadata.Types.OutputTarget.Window3: return OutputTargets.Window3;
                case Domain.Metadata.Types.OutputTarget.Window4: return OutputTargets.Window4;

                default: throw new ArgumentException($"Unsupported output target {target}");
            }
        }

        public static Domain.Drawable.Types.ObjectType ToDomainEnum(this DrawableObjectType type)
        {
            switch (type)
            {
                case DrawableObjectType.VerticalLine: return Domain.Drawable.Types.ObjectType.VerticalLine;
                case DrawableObjectType.HorizontalLine: return Domain.Drawable.Types.ObjectType.HorizontalLine;
                case DrawableObjectType.TrendLine: return Domain.Drawable.Types.ObjectType.TrendLine;
                case DrawableObjectType.Rectangle: return Domain.Drawable.Types.ObjectType.Rectangle;
                case DrawableObjectType.Triangle: return Domain.Drawable.Types.ObjectType.Triangle;
                case DrawableObjectType.Ellipse: return Domain.Drawable.Types.ObjectType.Ellipse;
                case DrawableObjectType.Symbol: return Domain.Drawable.Types.ObjectType.Symbol;
                case DrawableObjectType.Text: return Domain.Drawable.Types.ObjectType.Text;
                case DrawableObjectType.Bitmap: return Domain.Drawable.Types.ObjectType.Bitmap;
                case DrawableObjectType.LabelControl: return Domain.Drawable.Types.ObjectType.LabelControl;
                case DrawableObjectType.RectangleControl: return Domain.Drawable.Types.ObjectType.RectangleControl;
                case DrawableObjectType.EditControl: return Domain.Drawable.Types.ObjectType.EditControl;
                case DrawableObjectType.ButtonControl: return Domain.Drawable.Types.ObjectType.ButtonControl;
                case DrawableObjectType.BitmapControl: return Domain.Drawable.Types.ObjectType.BitmapControl;
                case DrawableObjectType.TextBlockControl: return Domain.Drawable.Types.ObjectType.TextBlockControl;
                case DrawableObjectType.Levels: return Domain.Drawable.Types.ObjectType.Levels;
                case DrawableObjectType.Cycles: return Domain.Drawable.Types.ObjectType.Cycles;
                case DrawableObjectType.LinRegChannel: return Domain.Drawable.Types.ObjectType.LinRegChannel;
                case DrawableObjectType.StdDevChannel: return Domain.Drawable.Types.ObjectType.StdDevChannel;
                case DrawableObjectType.EquidistantChannel: return Domain.Drawable.Types.ObjectType.EquidistantChannel;
                case DrawableObjectType.GannLine: return Domain.Drawable.Types.ObjectType.GannLine;
                case DrawableObjectType.GannFan: return Domain.Drawable.Types.ObjectType.GannFan;
                case DrawableObjectType.GannGrid: return Domain.Drawable.Types.ObjectType.GannGrid;
                case DrawableObjectType.FiboFan: return Domain.Drawable.Types.ObjectType.FiboFan;
                case DrawableObjectType.FiboArcs: return Domain.Drawable.Types.ObjectType.FiboArcs;
                case DrawableObjectType.FiboChannel: return Domain.Drawable.Types.ObjectType.FiboChannel;
                case DrawableObjectType.FiboRetracement: return Domain.Drawable.Types.ObjectType.FiboRetracement;
                case DrawableObjectType.FiboTimeZones: return Domain.Drawable.Types.ObjectType.FiboTimeZones;
                case DrawableObjectType.FiboExpansion: return Domain.Drawable.Types.ObjectType.FiboExpansion;
                case DrawableObjectType.AndrewsPitchfork: return Domain.Drawable.Types.ObjectType.AndrewsPitchfork;
                default: throw new ArgumentException("Unknown object type");
            }
        }

        public static Domain.Drawable.Types.SymbolAnchor ToDomainEnum(this DrawableSymbolAnchor anchor)
        {
            switch (anchor)
            {
                case DrawableSymbolAnchor.Top: return Domain.Drawable.Types.SymbolAnchor.Top;
                case DrawableSymbolAnchor.Bottom: return Domain.Drawable.Types.SymbolAnchor.Bottom;
                default: throw new ArgumentException("Unknown symbol anchor");
            }
        }

        public static DrawableSymbolAnchor ToApiEnum(this Domain.Drawable.Types.SymbolAnchor anchor)
        {
            switch (anchor)
            {
                case Domain.Drawable.Types.SymbolAnchor.Top: return DrawableSymbolAnchor.Top;
                case Domain.Drawable.Types.SymbolAnchor.Bottom: return DrawableSymbolAnchor.Bottom;
                default: throw new ArgumentException("Unknown symbol anchor");
            }
        }

        public static Domain.Drawable.Types.ControlZeroPosition ToDomainEnum(this DrawableControlZeroPosition pos)
        {
            switch (pos)
            {
                case DrawableControlZeroPosition.UpperLeft: return Domain.Drawable.Types.ControlZeroPosition.UpperLeft;
                case DrawableControlZeroPosition.UpperRight: return Domain.Drawable.Types.ControlZeroPosition.UpperRight;
                case DrawableControlZeroPosition.LowerLeft: return Domain.Drawable.Types.ControlZeroPosition.LowerLeft;
                case DrawableControlZeroPosition.LowerRight: return Domain.Drawable.Types.ControlZeroPosition.LowerRight;
                default: throw new ArgumentException("Unknown control zero position");
            }
        }

        public static DrawableControlZeroPosition ToApiEnum(this Domain.Drawable.Types.ControlZeroPosition pos)
        {
            switch (pos)
            {
                case Domain.Drawable.Types.ControlZeroPosition.UpperLeft: return DrawableControlZeroPosition.UpperLeft;
                case Domain.Drawable.Types.ControlZeroPosition.UpperRight: return DrawableControlZeroPosition.UpperRight;
                case Domain.Drawable.Types.ControlZeroPosition.LowerLeft: return DrawableControlZeroPosition.LowerLeft;
                case Domain.Drawable.Types.ControlZeroPosition.LowerRight: return DrawableControlZeroPosition.LowerRight;
                default: throw new ArgumentException("Unknown control zero position");
            }
        }

        public static Domain.Drawable.Types.PositionMode ToDomainEnum(this DrawablePositionMode mode)
        {
            switch (mode)
            {
                case DrawablePositionMode.TopLeft: return Domain.Drawable.Types.PositionMode.TopLeft;
                case DrawablePositionMode.TopCenter: return Domain.Drawable.Types.PositionMode.TopCenter;
                case DrawablePositionMode.TopRight: return Domain.Drawable.Types.PositionMode.TopRight;
                case DrawablePositionMode.CenterLeft: return Domain.Drawable.Types.PositionMode.CenterLeft;
                case DrawablePositionMode.Center: return Domain.Drawable.Types.PositionMode.Center;
                case DrawablePositionMode.CenterRight: return Domain.Drawable.Types.PositionMode.CenterRight;
                case DrawablePositionMode.BottomLeft: return Domain.Drawable.Types.PositionMode.BottomLeft;
                case DrawablePositionMode.BottomCenter: return Domain.Drawable.Types.PositionMode.BottomCenter;
                case DrawablePositionMode.BottomRight: return Domain.Drawable.Types.PositionMode.BottomRight;
                default: throw new ArgumentException("Unknown position mode");
            }
        }

        public static DrawablePositionMode ToApiEnum(this Domain.Drawable.Types.PositionMode mode)
        {
            switch (mode)
            {
                case Domain.Drawable.Types.PositionMode.TopLeft: return DrawablePositionMode.TopLeft;
                case Domain.Drawable.Types.PositionMode.TopCenter: return DrawablePositionMode.TopCenter;
                case Domain.Drawable.Types.PositionMode.TopRight: return DrawablePositionMode.TopRight;
                case Domain.Drawable.Types.PositionMode.CenterLeft: return DrawablePositionMode.CenterLeft;
                case Domain.Drawable.Types.PositionMode.Center: return DrawablePositionMode.Center;
                case Domain.Drawable.Types.PositionMode.CenterRight: return DrawablePositionMode.CenterRight;
                case Domain.Drawable.Types.PositionMode.BottomLeft: return DrawablePositionMode.BottomLeft;
                case Domain.Drawable.Types.PositionMode.BottomCenter: return DrawablePositionMode.BottomCenter;
                case Domain.Drawable.Types.PositionMode.BottomRight: return DrawablePositionMode.BottomRight;
                default: throw new ArgumentException("Unknown position mode");
            }
        }

        public static Domain.Drawable.Types.GannDirection ToDomainEnum(this DrawableGannDirection direction)
        {
            switch (direction)
            {
                case DrawableGannDirection.UpTrend: return Domain.Drawable.Types.GannDirection.UpTrend;
                case DrawableGannDirection.DownTrend: return Domain.Drawable.Types.GannDirection.DownTrend;
                default: throw new ArgumentException("Unknown gann direction");
            }
        }

        public static DrawableGannDirection ToApiEnum(this Domain.Drawable.Types.GannDirection direction)
        {
            switch (direction)
            {
                case Domain.Drawable.Types.GannDirection.UpTrend: return DrawableGannDirection.UpTrend;
                case Domain.Drawable.Types.GannDirection.DownTrend: return DrawableGannDirection.DownTrend;
                default: throw new ArgumentException("Unknown gann direction");
            }
        }
    }
}