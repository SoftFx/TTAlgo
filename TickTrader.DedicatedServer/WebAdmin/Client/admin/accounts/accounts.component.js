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
var index_1 = require("../../services/index");
var index_2 = require("../../models/index");
var AccountsComponent = (function () {
    function AccountsComponent(_api, _toastr) {
        this._api = _api;
        this._toastr = _toastr;
        this.Account = new index_2.AccountModel();
    }
    AccountsComponent.prototype.ngOnInit = function () {
        var _this = this;
        this.Accounts = [];
        this._api.Feed.addAccount.subscribe(function (acc) { _this.Accounts.push(acc); });
        this._api.Feed.deleteAccount.subscribe(function (acc) { _this.Accounts = _this.Accounts.filter(function (a) { return !(a.Login === acc.Login && a.Server === acc.Server); }); });
        this._api.GetAccounts()
            .subscribe(function (result) { return _this.Accounts = result; });
    };
    AccountsComponent.prototype.Add = function () {
        var _this = this;
        var accountClone = Object.assign(new index_2.AccountModel(), this.Account);
        this._api.AddAccount(accountClone)
            .subscribe(function (ok) { return _this.Cancel(); }, function (err) {
            _this._toastr.error(err.Message);
        });
    };
    AccountsComponent.prototype.Cancel = function () {
        this.Account = new index_2.AccountModel();
    };
    AccountsComponent.prototype.Test = function () {
        var _this = this;
        var accountClone = Object.assign(new index_2.AccountModel(), this.Account);
        this.ConnectionTestRunning = true;
        this.ResetTestResult();
        this._api.TestAccount(accountClone)
            .finally(function () { _this.ConnectionTestRunning = false; })
            .subscribe(function (ok) { return _this.TestResult = new index_2.ConnectionTestResult(ok.json()); });
    };
    AccountsComponent.prototype.ResetTestResult = function () {
        this.TestResult = null;
    };
    return AccountsComponent;
}());
AccountsComponent = __decorate([
    core_1.Component({
        selector: 'accounts-cmp',
        template: require('./accounts.component.html'),
        styles: [require('../../app.component.css')]
    }),
    __metadata("design:paramtypes", [index_1.ApiService, index_1.ToastrService])
], AccountsComponent);
exports.AccountsComponent = AccountsComponent;
//# sourceMappingURL=accounts.component.js.map