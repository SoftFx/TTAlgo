﻿using Machinarium.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Common
{
    public enum RateChangeDirections
    {
        Unknown,
        Up,
        Down,
    }

    public class RateDirectionTracker : ObservableObject
    {
        private double? rate;
        private int precision;
        //private string rateFormat;

        public double? Rate
        {
            get { return rate; }
            internal set
            {
                if (rate == null || value == null)
                    Direction = RateChangeDirections.Unknown;
                else if (rate.Value < value)
                    Direction = RateChangeDirections.Up;
                else if (rate.Value > value)
                    Direction = RateChangeDirections.Down;

                this.rate = value;

                OnPropertyChanged(nameof(Rate));
                OnPropertyChanged(nameof(Direction));
            }
        }

        public int Precision
        {
            get { return precision; }
            set
            {
                precision = value;
                OnPropertyChanged(nameof(Precision));
            }
        }

        public RateChangeDirections Direction { get; private set; }
    }
}