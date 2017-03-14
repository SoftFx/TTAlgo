﻿using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal enum RateChangeDirections
    {
        Unknown,
        Up,
        Down,
    }

    internal class RateDirectionTracker : PropertyChangedBase
    {
        private double? rate;
        private int precision;
        private string rateFormat;

        public double? Rate
        {
            get { return rate; }
            set
            {
                if (rate == null || value == null)
                    Direction = RateChangeDirections.Unknown;
                else if (rate.Value < value)
                    Direction = RateChangeDirections.Up;
                else if (rate.Value > value)
                    Direction = RateChangeDirections.Down;

                this.rate = value;

                NotifyOfPropertyChange(nameof(Rate));
                NotifyOfPropertyChange(nameof(Direction));
            }
        }

        public int Precision
        {
            get { return precision; }
            set
            {
                precision = value;
                NotifyOfPropertyChange(nameof(Precision));
            }
        }

        public RateChangeDirections Direction { get; private set; }
    }
}