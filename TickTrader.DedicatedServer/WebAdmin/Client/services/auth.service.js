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
var index_1 = require("../models/index");
var Rx_1 = require("rxjs/Rx");
var feed_service_1 = require("./feed.service");
var BehaviorSubject_1 = require("rxjs/BehaviorSubject");
var AuthService = (function () {
    function AuthService(_http, _feed) {
        var _this = this;
        this._http = _http;
        this._feed = _feed;
        this._storageKey = 'a-token';
        this._loginUrl = '/api/Login';
        this._authDataUpdatedSubject = new BehaviorSubject_1.BehaviorSubject(null);
        this.AuthDataUpdated = this._authDataUpdatedSubject.asObservable();
        this.AuthDataUpdated.subscribe(function (authData) {
            if (authData && _this.IsAuthorized()) {
                Rx_1.Observable.of(authData).delay(1500).subscribe(function (auth) {
                    _this._feed.start(true, authData.token).subscribe(null, function (error) { return console.log('Error on init: ' + error); });
                });
            }
        });
        this.restoreAuthData();
    }
    AuthService.prototype.IsAuthorized = function () {
        if (localStorage.getItem(this._storageKey)) {
            var authData = JSON.parse(localStorage.getItem(this._storageKey), index_1.Utils.DateReviver);
            var nowUtc = new Date(new Date().toUTCString());
            if (authData.expires >= nowUtc)
                return true;
        }
        return false;
    };
    Object.defineProperty(AuthService.prototype, "AuthData", {
        get: function () {
            var authData = JSON.parse(localStorage.getItem(this._storageKey), index_1.Utils.DateReviver);
            return authData;
        },
        enumerable: true,
        configurable: true
    });
    AuthService.prototype.LogIn = function (login, password) {
        var _this = this;
        return this._http.post(this._loginUrl, { Login: login, Password: password })
            .map(function (response) { return response.json(); })
            .do(function (authData) {
            if (authData && authData.token) {
                localStorage.setItem(_this._storageKey, JSON.stringify(authData));
                _this._authDataUpdatedSubject.next(authData);
                return Rx_1.Observable.of(true);
            }
            return Rx_1.Observable.of(false);
        });
    };
    AuthService.prototype.LogOut = function () {
        this._feed.stop().subscribe(null, function (error) { return console.log('Error on init: ' + error); });
        localStorage.removeItem(this._storageKey);
        this._authDataUpdatedSubject.next(null);
    };
    AuthService.prototype.restoreAuthData = function () {
        var authData = JSON.parse(localStorage.getItem(this._storageKey), index_1.Utils.DateReviver);
        if (this.IsAuthorized()) {
            this._authDataUpdatedSubject.next(authData);
        }
        else {
            localStorage.removeItem(this._storageKey);
        }
    };
    return AuthService;
}());
AuthService = __decorate([
    core_1.Injectable(),
    __metadata("design:paramtypes", [http_1.Http, feed_service_1.FeedService])
], AuthService);
exports.AuthService = AuthService;
//# sourceMappingURL=auth.service.js.map