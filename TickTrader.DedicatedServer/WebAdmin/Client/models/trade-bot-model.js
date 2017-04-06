"use strict";
var account_model_1 = require("./account-model");
var TradeBotModel = (function () {
    function TradeBotModel() {
    }
    TradeBotModel.prototype.Deserialize = function (input) {
        this.Id = input.Id;
        this.Status = input.Status;
        this.Account = new account_model_1.AccountModel().Deserialize(input.Account);
        this.State = TradeBotStates[input.State];
        return this;
    };
    return TradeBotModel;
}());
exports.TradeBotModel = TradeBotModel;
var TradeBotStates;
(function (TradeBotStates) {
    TradeBotStates[TradeBotStates["Offline"] = 0] = "Offline";
    TradeBotStates[TradeBotStates["Started"] = 1] = "Started";
    TradeBotStates[TradeBotStates["Initializing"] = 2] = "Initializing";
    TradeBotStates[TradeBotStates["Faulted"] = 3] = "Faulted";
    TradeBotStates[TradeBotStates["Online"] = 4] = "Online";
    TradeBotStates[TradeBotStates["Stopping"] = 5] = "Stopping";
})(TradeBotStates = exports.TradeBotStates || (exports.TradeBotStates = {}));
//# sourceMappingURL=trade-bot-model.js.map