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
        private readonly TelegramBotUpdateHandler _handler = new();

        private CancellationToken _cToken = CancellationToken.None;
        private TelegramBotClientOptions _cOptions;
        private TelegramBotClient _bot;

        private bool _isRun = false;


        internal async Task StartBot(TelegramSettings settings)
        {
            if (_isRun && !IsNewSettings(settings))
                return;

            await StopBot();

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

                _isRun = true;

                _logger.Info($"{BotName} has been started");
            }
        }

        internal async Task StopBot()
        {
            if (!_isRun)
                return;

            _logger.Info($"{BotName} is stopping");

            _isRun = false;

            _cToken.ThrowIfCancellationRequested();

            await (_bot?.CloseAsync() ?? Task.CompletedTask);

            _logger.Info($"{BotName} has been stopped");
        }

        internal Task ApplySettings(TelegramSettings settings)
        {
            return settings.Enable ? StartBot(settings) : StopBot();
        }

        internal async Task SendMessage(string message)
        {
            try
            {
                foreach ((_, var chat) in _handler.Chats)
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