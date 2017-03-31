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
var Rx_1 = require("rxjs/Rx");
var BehaviorSubject_1 = require("rxjs/BehaviorSubject");
var index_1 = require("../models/index");
var http_1 = require("@angular/http");
var feed_service_1 = require("./feed.service");
var ApiService = (function () {
    function ApiService(_http, Feed) {
        this._http = _http;
        this.Feed = Feed;
        this.headers = new http_1.Headers({ 'Content-Type': 'application/json' });
        this.repositoryUrl = '/api/Repository';
        this.accountsUrl = '/api/Account';
        this.testAccountUrl = '/api/TestAccount';
        this.dashboardUrl = '/api/Dashboard';
        this.dataStore = { bots: [] };
        this._bots = new BehaviorSubject_1.BehaviorSubject([]);
        this.dasboardBots = this._bots.asObservable();
    }
    ApiService.prototype.loadAllBots = function () {
        return Rx_1.Observable.of(index_1.FakeData.bots);
    };
    ApiService.prototype.loadBotsOnDashboard = function () {
        var _this = this;
        Rx_1.Observable.of(index_1.FakeData.extBots).delay(150).subscribe(function (data) {
            _this.dataStore.bots = data;
            _this._bots.next(Object.assign({}, _this.dataStore).bots);
        }, function (error) { return console.log('Could not load bots.'); });
    };
    ApiService.prototype.getBot = function (id) {
        return Rx_1.Observable.of(this.dataStore.bots.find(function (bot) { return bot.instanceId == id; }));
    };
    ApiService.prototype.removeBotFromDashboard = function (bot) {
        var _this = this;
        Rx_1.Observable.of(true).delay(150).subscribe(function (response) {
            _this.dataStore.bots.forEach(function (b, i) {
                if (b == bot) {
                    _this.dataStore.bots.splice(i, 1);
                }
            });
            _this._bots.next(Object.assign({}, _this.dataStore).bots);
        }, function (error) { return console.log('Could not delete bot.'); });
    };
    ApiService.prototype.addBot = function (bot) {
        var _this = this;
        return Rx_1.Observable
            .of(true)
            .delay(150)
            .do(function (response) {
            _this.dataStore.bots.push(bot);
        }, function (err) { }, function () { });
    };
    ApiService.prototype.runBot = function (bot) {
        var _this = this;
        return Rx_1.Observable.from([index_1.BotState.Running, index_1.BotState.Runned])
            .map(function (value) { return Rx_1.Observable.of(value).delay(value == index_1.BotState.Running ? 0 : 1500); })
            .concatAll()
            .subscribe(function (response) {
            _this.updateBotState(bot, response);
        }, function (err) { }, function () { });
    };
    ApiService.prototype.stopBot = function (bot) {
        var _this = this;
        return Rx_1.Observable.from([index_1.BotState.Stopping, index_1.BotState.Stopped])
            .map(function (value) { return Rx_1.Observable.of(value).delay(value == index_1.BotState.Stopping ? 0 : 1500); })
            .concatAll()
            .subscribe(function (response) {
            _this.updateBotState(bot, response);
        }, function (err) { }, function () { });
    };
    ApiService.prototype.SetupPlugin = function (setup) {
        return this._http.post(this.dashboardUrl, setup.Payload, { headers: this.headers })
            .catch(this.handleServerError);
    };
    /* >>> API Repository*/
    ApiService.prototype.UploadPackage = function (file) {
        var input = new FormData();
        input.append("file", file);
        return this._http
            .post(this.repositoryUrl, input)
            .catch(this.handleServerError);
    };
    ApiService.prototype.DeletePackage = function (name) {
        return this._http
            .delete(this.repositoryUrl + "/" + name, { headers: this.headers })
            .catch(this.handleServerError);
    };
    ApiService.prototype.GetPackages = function () {
        return this._http
            .get(this.repositoryUrl)
            .map(function (res) { return res.json().map(function (i) { return new index_1.PackageModel().Deserialize(i); }); })
            .catch(this.handleServerError);
    };
    /* <<< API Repository*/
    /* >>> API Accounts */
    ApiService.prototype.GetAccounts = function () {
        return this._http
            .get(this.accountsUrl)
            .map(function (res) { return res.json().map(function (i) { return new index_1.AccountModel().Deserialize(i); }); })
            .catch(this.handleServerError);
    };
    ApiService.prototype.AddAccount = function (acc) {
        return this._http
            .post(this.accountsUrl, acc, { headers: this.headers })
            .catch(this.handleServerError);
    };
    ApiService.prototype.DeleteAccount = function (acc) {
        return this._http
            .delete(this.accountsUrl + "/?" + $.param({ login: acc.Login, server: acc.Server }), { headers: this.headers })
            .catch(this.handleServerError);
    };
    ApiService.prototype.UpdateAccount = function (acc) {
        return Rx_1.Observable.throw('NotImplemented');
    };
    ApiService.prototype.TestAccount = function (acc) {
        return this._http.post(this.testAccountUrl, acc, { headers: this.headers })
            .catch(this.handleServerError);
    };
    /* <<< API Accounts */
    ApiService.prototype.GetSymbols = function (account) {
        return Rx_1.Observable.of(['EURUSD', 'AEDAUD', 'USDAFN', 'USDAMD']);
    };
    ApiService.prototype.updateBotState = function (bot, state) {
        for (var _i = 0, _a = this.dataStore.bots; _i < _a.length; _i++) {
            var cBot = _a[_i];
            if (cBot.instanceId == bot.instanceId) {
                cBot.state = state;
                return true;
            }
        }
        return false;
    };
    ApiService.prototype.handleServerError = function (error) {
        console.error('[ApiService] An error occurred' + error); //debug
        return Rx_1.Observable.throw(new index_1.ResponseStatus(error));
    };
    return ApiService;
}());
ApiService = __decorate([
    core_1.Injectable(),
    __metadata("design:paramtypes", [http_1.Http, feed_service_1.FeedService])
], ApiService);
exports.ApiService = ApiService;
//# sourceMappingURL=api.service.js.map