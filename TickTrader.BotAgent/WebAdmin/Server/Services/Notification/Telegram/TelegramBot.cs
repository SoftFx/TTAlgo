using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TickTrader.BotAgent.WebAdmin.Server.Settings;

namespace TickTrader.BotAgent.WebAdmin.Server.Services.Notification
{
    internal sealed class TelegramBot : IAsyncDisposable
    {
        private const string BotName = nameof(TelegramBot);

        private readonly Logger _logger = LogManager.GetLogger(nameof(TelegramBot));

        private readonly TelegramBotUpdateHandler _handler;
        private readonly NotificationStorage _storage;

        private CancellationToken _cToken = CancellationToken.None;
        private TelegramBotClientOptions _cOptions;
        private TelegramBotClient _bot;

        private bool _isRun = false;


        public TelegramBot(NotificationStorage storage)
        {
            _handler = new(storage);
            _storage = storage;
        }

        internal async Task StartBot(TelegramSettings settings)
        {
            if (_isRun && !IsNewSettings(settings))
                return;

            await StopBot();

            _isRun = true;

            _logger.Info($"{BotName} is starting");

            _cToken = new CancellationToken();
            _cOptions = new TelegramBotClientOptions(settings.BotToken);

            _bot = new TelegramBotClient(_cOptions)
            {
                Timeout = BotSettings.AvailableDelay,
            };

            if (await TestBotToken())
            {
                _bot.StartReceiving(_handler, BotSettings.Options, _cToken);

                await _bot.SetMyCommandsAsync(BotSettings.BotCommands, cancellationToken: _cToken);


                _logger.Info($"{BotName} has been started");
            }
            else
                _isRun = false;
        }

        internal async Task StopBot()
        {
            if (!_isRun)
                return;

            _logger.Info($"{BotName} is stopping");

            _isRun = false;

            _cToken.ThrowIfCancellationRequested();

            if (_bot != null)
            {
                await _bot.DeleteWebhookAsync();
                await _bot.CloseAsync();
            }

            _logger.Info($"{BotName} has been stopped");
        }

        internal Task ApplySettings(TelegramSettings settings)
        {
            if (settings.Enable == _isRun) //IOptionsMonitoring OnChange fired twice
                return Task.CompletedTask;

            return settings.Enable ? StartBot(settings) : StopBot();
        }

        internal async Task SendMessage(string message)
        {
            try
            {
                if (!_isRun)
                    return;

                foreach ((_, var chat) in _storage.Settings.Telegram.Chats)
                    await _bot.SendTextMessageAsync(chat, message, ParseMode.MarkdownV2, cancellationToken: _cToken);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex);
            }
        }


        private async Task<bool> TestBotToken()
        {
            try
            {
                await _bot.GetMeAsync(_cToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex);
            }

            return false;
        }

        private bool IsNewSettings(TelegramSettings settings)
        {
            return _cOptions is null || _cOptions.Token != settings.BotToken;
        }

        public async ValueTask DisposeAsync()
        {
            await StopBot();
        }
    }
}