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
var PluginSetupComponent = (function () {
    function PluginSetupComponent(_api) {
        this._api = _api;
        this.ParameterDataType = index_1.ParameterDataTypes;
    }
    PluginSetupComponent.prototype.OnAccountChanged = function (account) {
        var _this = this;
        this._api.GetSymbols(account).subscribe(function (symbols) { return _this.Symbols = symbols; });
    };
    return PluginSetupComponent;
}());
__decorate([
    core_1.Input(),
    __metadata("design:type", index_1.PluginSetupModel)
], PluginSetupComponent.prototype, "Setup", void 0);
__decorate([
    core_1.Input(),
    __metadata("design:type", Array)
], PluginSetupComponent.prototype, "Accounts", void 0);
PluginSetupComponent = __decorate([
    core_1.Component({
        selector: 'plugin-setup-cmp',
        template: require('./plugin-setup.component.html'),
        styles: [require('../../app.component.css')],
    }),
    __metadata("design:paramtypes", [index_2.ApiService])
], PluginSetupComponent);
exports.PluginSetupComponent = PluginSetupComponent;
//# sourceMappingURL=plugin-setup.component.js.map