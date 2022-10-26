using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.BotAgent.WebAdmin.Server.Services.Notification;
using TickTrader.BotAgent.WebAdmin.Server.Settings;

namespace TickTrader.BotAgent.WebAdmin.Server.Services
{
    public class NotificationService : BackgroundService, IAsyncDisposable
    {
        private readonly Channel<AlertRecordInfo> _channel = DefaultChannelFactory.CreateForOneToOne<AlertRecordInfo>();
        private readonly NotificationBuilder _builder = new();
        private readonly TelegramBot _telegramBot = new();

        private readonly IAlgoServerApi _serverApi;
        private readonly IDisposable _settingsSub;

        private NotificationSettings _currentSettings;


        public NotificationService(IAlgoServerApi server, IOptionsMonitor<NotificationSettings> file)
        {
            _currentSettings = file.CurrentValue;
            _serverApi = server;

            _settingsSub = file.OnChange(async s => await ApplyNewSettings(s));
        }


        public override async Task StartAsync(CancellationToken cToken)
        {
            await _serverApi.SubscribeToAlerts(_channel.Writer);
            await ApplyNewSettings(_currentSettings);

            _ = _channel.Consume(_builder.AddAlert, cancelToken: cToken);

            await base.StartAsync(cToken);
        }

        public override async Task StopAsync(CancellationToken cToken)
        {
            await DisposeAsync();
            await base.StopAsync(cToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cToken)
        {
            while (!cToken.IsCancellationRequested)
            {
                if (!_builder.IsEmpty && _currentSettings.Enable)
                {
                    var message = _builder.GetMessage();

                    await _telegramBot.SendMessage(message);
                }

                await Task.Delay(_currentSettings.MinDelay, cToken);
            }
        }


        private Task ApplyNewSettings(NotificationSettings settings)
        {
            _currentSettings = settings;

            return _telegramBot.ApplySettings(settings.Telegram);
        }

        public ValueTask DisposeAsync()
        {
            _channel.Writer?.TryComplete();
            _settingsSub?.Dispose();

            return _telegramBot.DisposeAsync();
        }
    }
}