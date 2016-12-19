import * as models from './index';
import { Guid } from './guid';

export class FakeData {
    static readonly bots: models.BotModel[] = [
        new models.BotModel(1, "Advanced Take Profi"),
        new models.BotModel(2, "MMA"),
        new models.BotModel(3, "Trade Tweeter"),
        new models.BotModel(4, "cMuti"),
        new models.BotModel(5, "News Robot"),
        new models.BotModel(6, "RSI bot"),
        new models.BotModel(7, "SAR Tralling"),
        new models.BotModel(8, "Trend"),
        new models.BotModel(9, "Robot_Forex"),
        new models.BotModel(10, "Close Profitable Positions"),
        new models.BotModel(11, "Macd Bot"),
        new models.BotModel(12, "Black Corvette"),
        new models.BotModel(13, "Jingle Bells"),
        new models.BotModel(14, "Skyfall Bot"),
        new models.BotModel(15, "Grinch Bot"),
        new models.BotModel(16, "Fake Bot"),
        new models.BotModel(17, "Super Fake Bot"),
        new models.BotModel(18, "ABAW Bot"),
        new models.BotModel(19, "Nutolf Bot"),
        new models.BotModel(20, "Albolia Bot"),
        new models.BotModel(21, "Gizigo Bot")
    ];

    static readonly extBots: models.ExtBotModel[] = [
        new models.ExtBotModel(1, "Advanced Take Profi", Guid.new(), true),
        new models.ExtBotModel(2, "MMA", Guid.new(), true),
        new models.ExtBotModel(3, "Trade Tweeter", Guid.new())
    ];
}