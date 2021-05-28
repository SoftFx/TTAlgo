using System;
using System.Windows;

namespace TickTrader.BotTerminal
{
    internal interface IDragAdorner: IAdornerDropState, IAdornerPosition, IDisposable {
    }

    internal interface IAdornerDropState
    {
        DropState DropState { get; set; }
    }

    internal interface IAdornerPosition
    {
        Point Position { get; set; }
    }
}