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
var ChangePasswordComponent = (function () {
    function ChangePasswordComponent(_api, _toastr) {
        this._api = _api;
        this._toastr = _toastr;
        this.OnChanged = new core_1.EventEmitter();
        this.OnCanceled = new core_1.EventEmitter();
    }
    ChangePasswordComponent.prototype.ChangePassword = function () {
        var _this = this;
        var account = new index_1.AccountModel();
        account.Login = this.Account.Login;
        account.Server = this.Account.Server;
        account.Password = this.Password;
        this._api.ChangeAccountPassword(account)
            .subscribe(function (ok) { return _this.OnChanged.emit(); }, function (err) { return _this._toastr.error("Error changing account password " + account.Login + " (" + account.Server + ")"); });
    };
    ChangePasswordComponent.prototype.Cancel = function () {
        this.OnCanceled.emit();
    };
    return ChangePasswordComponent;
}());
__decorate([
    core_1.Input(),
    __metadata("design:type", index_1.AccountModel)
], ChangePasswordComponent.prototype, "Account", void 0);
__decorate([
    core_1.Output(),
    __metadata("design:type", Object)
], ChangePasswordComponent.prototype, "OnChanged", void 0);
__decorate([
    core_1.Output(),
    __metadata("design:type", Object)
], ChangePasswordComponent.prototype, "OnCanceled", void 0);
ChangePasswordComponent = __decorate([
    core_1.Component({
        selector: 'change-password-cmp',
        template: require('./change-password.component.html'),
        styles: [require('../../app.component.css')],
    }),
    __metadata("design:paramtypes", [index_2.ApiService, index_2.ToastrService])
], ChangePasswordComponent);
exports.ChangePasswordComponent = ChangePasswordComponent;
//# sourceMappingURL=change-password.component.js.map