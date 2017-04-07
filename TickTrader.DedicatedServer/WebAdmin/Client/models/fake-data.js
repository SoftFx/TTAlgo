"use strict";
var models = require("./index");
var FakeData = (function () {
    function FakeData() {
    }
    return FakeData;
}());
FakeData.numberParams = [
    models.Parameter.CreateNumberParameter("NumParameter1"),
    models.Parameter.CreateNumberParameter("NumParameter2", 200, "Num Parameter Two"),
    models.Parameter.CreateNumberParameter("NumParameter3", 1.1351, "Num Parameter Three"),
    models.Parameter.CreateNumberParameter("NumParameter4", 0.8542, "Num Parameter Three"),
    models.Parameter.CreateNumberParameter("NumParameter5", 1.1132),
    models.Parameter.CreateNumberParameter("NumParameter7", 1.1132, "Num Parameter Seven"),
];
FakeData.stringParams = [
    models.Parameter.CreateStringParameter("StrParameter"),
    models.Parameter.CreateStringParameter("StrParameter2", "str param 2", "Parameter Two"),
    models.Parameter.CreateStringParameter("StrParameter3", "str param 3", "Parameter Three"),
    models.Parameter.CreateStringParameter("StrParameter4"),
    models.Parameter.CreateStringParameter("StrParameter5"),
    models.Parameter.CreateStringParameter("StrParameter7"),
];
FakeData.enumParametrs = [
    models.Parameter.CreateEnumParametr("EnumParameter1", JSON.parse('{"EnumValue1": 1, "EnumValue2": 2, "EnumValue3": 3}')),
    models.Parameter.CreateEnumParametr("EnumParameter2", JSON.parse('{"AnotherEnumValue1": 1, "AnotherEnumValue2": 2, "AnotherEnumValue3": 3}'))
];
FakeData.bots = [
    new models.BotModel("Bot with one parameters", new models.BotSetup(FakeData.numberParams[0])),
    new models.BotModel("Bot with two parameters", new models.BotSetup(FakeData.numberParams[1], FakeData.stringParams[2])),
    new models.BotModel("Bot with three parameters", new models.BotSetup(FakeData.numberParams[0], FakeData.numberParams[3], FakeData.stringParams[0])),
    new models.BotModel("Bot with four parameters", new models.BotSetup(FakeData.numberParams[0], FakeData.numberParams[3], FakeData.stringParams[0], FakeData.stringParams[3])),
    new models.BotModel("Bot with five parameters", new models.BotSetup(FakeData.stringParams[1], FakeData.stringParams[2], FakeData.stringParams[3], FakeData.numberParams[0], FakeData.numberParams[1])),
];
FakeData.extBots = [
    new models.ExtBotModel(FakeData.bots[0].name, FakeData.bots[0].setup),
    new models.ExtBotModel(FakeData.bots[1].name, FakeData.bots[1].setup, "", true),
    new models.ExtBotModel(FakeData.bots[2].name, FakeData.bots[2].setup, "", false),
    new models.ExtBotModel(FakeData.bots[3].name, FakeData.bots[3].setup, "", true),
    new models.ExtBotModel(FakeData.bots[3].name, FakeData.bots[4].setup, "", true),
    new models.ExtBotModel(FakeData.bots[3].name, FakeData.bots[4].setup, "", true),
    new models.ExtBotModel(FakeData.bots[3].name, FakeData.bots[4].setup, "", true),
    new models.ExtBotModel(FakeData.bots[3].name, FakeData.bots[1].setup, "", true),
    new models.ExtBotModel(FakeData.bots[4].name, FakeData.bots[0].setup, "", true)
];
exports.FakeData = FakeData;
//# sourceMappingURL=fake-data.js.map