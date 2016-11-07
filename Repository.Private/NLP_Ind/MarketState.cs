using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLP_Ind
{
    internal class MarketState
    {
        public bool TrendUP { get; private set; } 
        public int MinAsk { get; private set; }
        public int MaxBid { get; private set; }
        public DateTime Prev1Time { get; private set; }
        public DateTime Prev2Time { get; private set; }

        public int PrevDuration { get; private set; }
        public int PrevAmplitude { get; private set; }
        public int CurrentDuration { get; private set; }
        public int CurrentAmplitude { get; private set; }

        public int Sensitivity {get;set;}

        public MarketState(int sensitivity)
        {
            this.Sensitivity = sensitivity;
            MinAsk = int.MaxValue;
            TrendUP = true;
            Prev1Time = DateTime.MinValue;
            Prev2Time = DateTime.MinValue;

        }

        public void ProcessQuote (int pipBid, int pipAsk, DateTime currTime)
        {
            if (TrendUP)
            {
                if (pipBid > MaxBid)
                {
                    MaxBid = pipBid;
                    Prev2Time = currTime;
                }
                else if (MaxBid - pipAsk >= Sensitivity)
                {
                    PrevAmplitude = MaxBid - MinAsk;
                    PrevDuration = (int)(Prev2Time - Prev1Time).TotalMinutes;
                    Prev1Time = Prev2Time;
                    Prev2Time = currTime;

                    TrendUP = false;
                    MinAsk = pipAsk;
                }
            }
            else
            {
                if (pipAsk < MinAsk)
                {
                    MinAsk = pipAsk;
                    Prev2Time = currTime;
                }
                else if (pipBid - MinAsk >= Sensitivity)
                {
                    PrevAmplitude = MaxBid - MinAsk;
                    PrevDuration = (int)(Prev2Time - Prev1Time).TotalMinutes;
                    Prev1Time = Prev2Time;
                    Prev2Time = currTime;

                    TrendUP = true;
                    MaxBid = pipBid;
                }
            }

            CurrentDuration = (int)(currTime - Prev2Time).TotalMinutes;
            CurrentAmplitude = MaxBid - MinAsk;
        }
    }
}
