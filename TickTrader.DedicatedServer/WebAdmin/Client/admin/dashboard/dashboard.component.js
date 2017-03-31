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
var DashboardComponent = (function () {
    function DashboardComponent(api, route, router) {
        this.api = api;
        this.route = route;
        this.router = router;
        this.botStateType = index_1.BotState;
        this.parameterType = index_1.ParameterType;
    }
    DashboardComponent.prototype.ngOnInit = function () {
        var _this = this;
        this.dashboardBots = this.api.dasboardBots;
        this.api.loadBotsOnDashboard();
        this.api.loadAllBots().subscribe(function (data) {
            _this.allBots = data;
        });
    };
    DashboardComponent.prototype.run = function (bot) {
        this.api.runBot(bot);
    };
    DashboardComponent.prototype.stop = function (bot) {
        this.api.stopBot(bot);
    };
    DashboardComponent.prototype.configurate = function (bot) {
    };
    DashboardComponent.prototype.remove = function (bot) {
        this.api.removeBotFromDashboard(bot);
    };
    DashboardComponent.prototype.gotoDetails = function (bot) {
        this.router.navigate(['/bot', bot.instanceId]);
    };
    return DashboardComponent;
}());
DashboardComponent = __decorate([
    core_1.Component({
        selector: 'dashboard-cmp',
        template: require('./dashboard.component.html'),
        styles: [require('../../app.component.css')],
    }),
    __metadata("design:paramtypes", [index_2.ApiService,
        router_1.ActivatedRoute,
        router_1.Router])
], DashboardComponent);
exports.DashboardComponent = DashboardComponent;
//# sourceMappingURL=dashboard.component.js.map