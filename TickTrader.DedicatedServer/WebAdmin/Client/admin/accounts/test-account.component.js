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
var TestAccountComponent = (function () {
    function TestAccountComponent(_api) {
        this._api = _api;
        this.ConnectionErrorCode = index_1.ConnectionErrorCodes;
        this.OnTested = new core_1.EventEmitter();
        this.OnClosed = new core_1.EventEmitter();
    }
    TestAccountComponent.prototype.ngOnInit = function () {
        this.resetTestResult();
        this.runTest();
    };
    TestAccountComponent.prototype.Close = function () {
        this.OnClosed.emit();
    };
    TestAccountComponent.prototype.runTest = function () {
        var _this = this;
        var accountClone = Object.assign(new index_1.AccountModel(), this.Account);
        this.ConnectionTestRunning = true;
        this._api.TestAccount(accountClone)
            .finally(function () { _this.ConnectionTestRunning = false; })
            .subscribe(function (ok) {
            _this.TestResult = new index_1.ConnectionTestResult(ok.json());
            _this.OnTested.emit(_this.TestResult);
        });
    };
    TestAccountComponent.prototype.resetTestResult = function () {
        this.TestResult = null;
    };
    return TestAccountComponent;
}());
__decorate([
    core_1.Input(),
    __metadata("design:type", index_1.AccountModel)
], TestAccountComponent.prototype, "Account", void 0);
__decorate([
    core_1.Output(),
    __metadata("design:type", Object)
], TestAccountComponent.prototype, "OnTested", void 0);
__decorate([
    core_1.Output(),
    __metadata("design:type", Object)
], TestAccountComponent.prototype, "OnClosed", void 0);
TestAccountComponent = __decorate([
    core_1.Component({
        selector: 'test-account-cmp',
        template: require('./test-account.component.html'),
        styles: [require('../../app.component.css')],
    }),
    __metadata("design:paramtypes", [index_2.ApiService])
], TestAccountComponent);
exports.TestAccountComponent = TestAccountComponent;
//# sourceMappingURL=test-account.component.js.map