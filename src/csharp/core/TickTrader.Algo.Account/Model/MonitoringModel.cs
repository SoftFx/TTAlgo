using System;
using System.Timers;
using TickTrader.Algo.Account.Settings;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Account
{
    internal sealed class QuoteMonitoringModel
    {
        private readonly Timer _sender;
        private readonly QuoteStats _stats;
        private readonly ConnectionModel _connection;
        private readonly Action<string> _sendNotification;


        internal QuoteMonitoringModel(ConnectionModel connection, IQuoteMonitoring settings)
        {
            _connection = connection;
            _sendNotification = settings.NotificationMethod;

            _stats = new QuoteStats(settings.AccetableQuoteDelay);
            _sender = new Timer(settings.AlertsDelay * 60000);

            _sender.Elapsed += SendNotification;
        }


        internal void CheckQuoteDelay(QuoteInfo quote)
        {
            if (_stats.TryAddQuote(quote) && !_sender.Enabled)
                _sender.Start();
        }

        private void SendNotification(object sender, ElapsedEventArgs e)
        {
            _sendNotification?.Invoke($"Account {_connection.CurrentLogin}. {_stats}");
            _stats.ResetStats();
            _sender.Stop();
        }


        private sealed class QuoteStats
        {
            private readonly int _maxDelay;

            private QuoteInfo _worstQuote;
            private int _delaysReceived;


            internal QuoteStats(int maxDelay)
            {
                _maxDelay = maxDelay;
            }


            internal bool TryAddQuote(QuoteInfo quote)
            {
                var badQuote = quote.QuoteDelay > _maxDelay;

                if (badQuote)
                {
                    _delaysReceived++;

                    if (_worstQuote == null || quote.QuoteDelay > _worstQuote.QuoteDelay)
                        _worstQuote = quote;
                }

                return badQuote;
            }

            internal void ResetStats()
            {
                _worstQuote = null;
                _delaysReceived = 0;
            }

            public override string ToString()
            {
                return $"Delays have been found: {_delaysReceived}. Worst quote: {_worstQuote.GetDelayInfo()}";
            }
        }
    }
}
