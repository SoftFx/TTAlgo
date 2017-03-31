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
var router_1 = require("@angular/router");
var BotDetailComponent = (function () {
    function BotDetailComponent(route, router, api) {
        this.route = route;
        this.router = router;
        this.api = api;
        this.parameterType = index_1.ParameterType;
        this.botStateType = index_1.BotState;
    }
    BotDetailComponent.prototype.ngOnInit = function () {
        var _this = this;
        this.route.params
            .switchMap(function (params) { return _this.api.getBot(params['id']); })
            .subscribe(function (bot) { return _this.bot = bot; });
    };
    BotDetailComponent.prototype.run = function () {
        this.api.runBot(this.bot);
    };
    BotDetailComponent.prototype.stop = function () {
        this.api.stopBot(this.bot);
    };
    BotDetailComponent.prototype.configurate = function () {
    };
    BotDetailComponent.prototype.remove = function () {
        this.api.removeBotFromDashboard(this.bot);
        this.router.navigate(["/dasboard"]);
    };
    return BotDetailComponent;
}());
BotDetailComponent = __decorate([
    core_1.Component({
        selector: 'bot-detail-cmp',
        template: require('./bot-detail.component.html'),
        styles: [require('../../app.component.css')],
    }),
    __metadata("design:paramtypes", [router_1.ActivatedRoute,
        router_1.Router,
        index_2.ApiService])
], BotDetailComponent);
exports.BotDetailComponent = BotDetailComponent;
//# sourceMappingURL=bot-detail.component.js.map