"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var core_1 = require("@angular/core");
var index_1 = require("../../models/index");
var index_2 = require("../../services/index");
var TradeBotCardComponent = (function () {
    function TradeBotCardComponent(_api, _toastr) {
        this._api = _api;
        this._toastr = _toastr;
        this.TradeBotState = index_1.TradeBotStates;
    }
    TradeBotCardComponent.prototype.Start = function () {
        this._api.StartBot(this.TradeBot.Id).subscribe(function (r) { });
    };
    TradeBotCardComponent.prototype.Stop = function () {
        this._api.StopBot(this.TradeBot.Id).subscribe(function (r) { });
    };
    return TradeBotCardComponent;
}());
__decorate([
    core_1.Input(),
    __metadata("design:type", index_1.TradeBotModel)
], TradeBotCardComponent.prototype, "TradeBot", void 0);
TradeBotCardComponent = __decorate([
    core_1.Component({
        selector: 'trade-bot-card-cmp',
        template: require('./trade-bot-card.component.html'),
        styles: [require('../../app.component.css')],
    }),
    __metadata("design:paramtypes", [index_2.ApiService, index_2.ToastrService])
], TradeBotCardComponent);
exports.TradeBotCardComponent = TradeBotCardComponent;
//# sourceMappingURL=trade-bot-card.component.js.map