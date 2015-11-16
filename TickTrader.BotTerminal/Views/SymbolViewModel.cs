﻿using Caliburn.Micro;
using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    class SymbolViewModel : PropertyChangedBase, IRateUpdatesListener
    {
        public enum States { Collapsed, Expanded, ExpandedWithLevel2 }

        private SymbolModel model;
        private bool isSelected;
        private States currentState;

        public SymbolViewModel(SymbolModel model)
        {
            this.model = model;
            this.model.Subscribe(this);

            this.Bid = new RateDirectionTracker();
            this.Ask = new RateDirectionTracker();

            this.DetailsPanel = new SymbolDetailsViewModel(Bid, Ask);
            this.Level2Panel = new SymbolLevel2ViewModel();
            if (model.Descriptor.Features.IsColorSupported)
                Color = model.Descriptor.Color;
        }

        public string Name { get { return model.Name; } }
        public string Group { get { return "Forex"; } }
        public int Color { get; private set; }

        public RateDirectionTracker Bid { get; private set; }
        public RateDirectionTracker Ask { get; private set; }
        public int Depth { get; private set; }
        public SymbolDetailsViewModel DetailsPanel { get; private set; }
        public SymbolLevel2ViewModel Level2Panel { get; private set; }

        #region State Management

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    NotifyOfPropertyChange("IsSelected");
                    if (value)
                        State = States.Expanded;
                    else
                        State = States.Collapsed;
                }
            }
        }

        public States State
        {
            get { return currentState; }
            set
            {
                this.currentState = value;
                NotifyOfPropertyChange("State");
                NotifyOfPropertyChange("IsExpanded");
                NotifyOfPropertyChange("IsLevel2Visible");
            }
        }

        public bool IsExpanded { get { return (State == States.Expanded || State == States.ExpandedWithLevel2); } }
        public bool IsLevel2Visible { get { return State == States.ExpandedWithLevel2; } }

        public event System.Action DepthChanged = delegate { };

        public void TriggerState()
        {
            if (!isSelected)
                return;

            if (State == States.Collapsed)
                State = States.Expanded;
            else if (State == States.Expanded)
                State = States.ExpandedWithLevel2;
            else if (State == States.ExpandedWithLevel2)
                State = States.Collapsed;
        }

        #endregion

        public void OnRateUpdate(Quote tick)
        {
            if (tick.HasBid)
                Bid.Rate = tick.Bid;

            if (tick.HasAsk)
                Ask.Rate = tick.Ask;
        }

        public void OpenChart()
        {
            NewChartRequested(this.model.Descriptor.Name);
        }

        public event Action<string> NewChartRequested = delegate { };

        public void Close()
        {
            this.model.Unsubscribe(this);
        }
    }
}