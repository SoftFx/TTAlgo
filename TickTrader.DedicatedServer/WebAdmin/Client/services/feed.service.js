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
var http_1 = require("@angular/http");
require("rxjs/add/operator/toPromise");
var Subject_1 = require("rxjs/Subject");
require("../../../node_modules/signalr/jquery.signalR.js");
$.getScript('signalr/hubs');
var index_1 = require("../models/index");
var FeedService = (function () {
    function FeedService(_http) {
        this._http = _http;
        this.currentState = index_1.ConnectionStatus.Disconnected;
        this.connectionStateSubject = new Subject_1.Subject();
        this.deletePackageSubject = new Subject_1.Subject();
        this.addPackageSubject = new Subject_1.Subject();
        this.addAccountSubject = new Subject_1.Subject();
        this.deleteAccountSubject = new Subject_1.Subject();
        this.connectionState = this.connectionStateSubject.asObservable();
        this.deletePackage = this.deletePackageSubject.asObservable();
        this.addPackage = this.addPackageSubject.asObservable();
        this.addAccount = this.addAccountSubject.asObservable();
        this.deleteAccount = this.deleteAccountSubject.asObservable();
    }
    FeedService.prototype.start = function (debug, token) {
        var _this = this;
        if (token) {
            $.connection.hub.qs = { 'authorization-token': token };
        }
        $.connection.hub.logging = debug;
        var connection = $.connection;
        var feedHub = connection.dSFeed;
        feedHub.client.addPackage = function (x) { return _this.onAddPackage(new index_1.PackageModel().Deserialize(x)); };
        feedHub.client.deletePackage = function (x) { return _this.onDeletePackage(x); };
        feedHub.client.addAccount = function (x) { return _this.onAddAccount(new index_1.AccountModel().Deserialize(x)); };
        feedHub.client.deleteAccount = function (x) { return _this.onDeleteAccount(new index_1.AccountModel().Deserialize(x)); };
        $.connection.hub.start()
            .done(function (response) { return _this.setConnectionState(index_1.ConnectionStatus.Connected); })
            .fail(function (error) { return _this.connectionStateSubject.error(error); });
        return this.connectionState;
    };
    FeedService.prototype.stop = function () {
        $.connection.hub.stop(true, true);
        $.connection.hub.qs = {};
        this.setConnectionState(index_1.ConnectionStatus.Disconnected);
        return this.connectionState;
    };
    FeedService.prototype.setConnectionState = function (connectionState) {
        console.log('connection state changed to: ' + connectionState);
        this.currentState = connectionState;
        this.connectionStateSubject.next(connectionState);
    };
    FeedService.prototype.onAddPackage = function (algoPackage) {
        console.info('[FeedService] onAddPackage', algoPackage);
        this.addPackageSubject.next(algoPackage);
    };
    FeedService.prototype.onDeletePackage = function (name) {
        console.info('[FeedService] onDeletePackage', name);
        this.deletePackageSubject.next(name);
    };
    FeedService.prototype.onAddAccount = function (account) {
        console.info('[FeedService] onAddAccount', account);
        this.addAccountSubject.next(account);
    };
    FeedService.prototype.onDeleteAccount = function (account) {
        console.info('[FeedService] onDeleteAccount', account);
        this.deleteAccountSubject.next(account);
    };
    return FeedService;
}());
FeedService = __decorate([
    core_1.Injectable(),
    __metadata("design:paramtypes", [http_1.Http])
], FeedService);
exports.FeedService = FeedService;
//# sourceMappingURL=feed.service.js.map