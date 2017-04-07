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
var AccountCardComponent = (function () {
    function AccountCardComponent(_api, _toastr) {
        this._api = _api;
        this._toastr = _toastr;
        this.onDeleted = new core_1.EventEmitter();
        this.onUpdated = new core_1.EventEmitter();
    }
    /* Change Password */
    AccountCardComponent.prototype.ChangePassword = function () {
        this.ChangePasswordEnabled = true;
    };
    AccountCardComponent.prototype.PasswordChangedOrCnaceled = function () {
        this.ChangePasswordEnabled = false;
    };
    /*Test Account*/
    AccountCardComponent.prototype.TestAccount = function () {
        this.TestAccountEnabled = true;
    };
    AccountCardComponent.prototype.TestAccountClosed = function () {
        this.TestAccountEnabled = false;
    };
    AccountCardComponent.prototype.Delete = function () {
        var _this = this;
        this._api.DeleteAccount(Object.assign(new index_1.AccountModel(), this.Account))
            .subscribe(function (ok) { return _this.onDeleted.emit(_this.Account); }, function (err) { return _this._toastr.error("Error deleting account " + _this.Account.Login + " (" + _this.Account.Server + ")"); });
    };
    return AccountCardComponent;
}());
__decorate([
    core_1.Input(),
    __metadata("design:type", index_1.AccountModel)
], AccountCardComponent.prototype, "Account", void 0);
__decorate([
    core_1.Output(),
    __metadata("design:type", Object)
], AccountCardComponent.prototype, "onDeleted", void 0);
__decorate([
    core_1.Output(),
    __metadata("design:type", Object)
], AccountCardComponent.prototype, "onUpdated", void 0);
AccountCardComponent = __decorate([
    core_1.Component({
        selector: 'account-card-cmp',
        template: require('./account-card.component.html'),
        styles: [require('../../app.component.css')],
    }),
    __metadata("design:paramtypes", [index_2.ApiService, index_2.ToastrService])
], AccountCardComponent);
exports.AccountCardComponent = AccountCardComponent;
//# sourceMappingURL=account-card.component.js.map