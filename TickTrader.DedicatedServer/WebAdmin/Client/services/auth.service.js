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
var Rx_1 = require("rxjs/Rx");
var feed_service_1 = require("./feed.service");
var AuthService = (function () {
    function AuthService(feed, http) {
        this.feed = feed;
        this.http = http;
        this.storageKey = 'a-token';
        this.isLoggedIn = false;
    }
    AuthService.prototype.isAuthorized = function () {
        return this.isLoggedIn;
    };
    AuthService.prototype.logIn = function (username, password) {
        var _this = this;
        this.feed.start(true)
            .subscribe(null, function (error) { return console.log('Error on init: ' + error); });
        return Rx_1.Observable.of(true).delay(500).do(function (val) { return _this.isLoggedIn = true; });
    };
    AuthService.prototype.logOut = function () {
        var _this = this;
        this.feed.stop()
            .subscribe(null, function (error) { return console.log('Error on init: ' + error); });
        return Rx_1.Observable.of(true).do(function (v) { return _this.isLoggedIn = false; });
    };
    return AuthService;
}());
AuthService = __decorate([
    core_1.Injectable(),
    __metadata("design:paramtypes", [feed_service_1.FeedService, http_1.Http])
], AuthService);
exports.AuthService = AuthService;
//# sourceMappingURL=auth.service.js.map