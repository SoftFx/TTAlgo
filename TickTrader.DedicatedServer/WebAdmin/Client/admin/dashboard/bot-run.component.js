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
var BotRunComponent = (function () {
    function BotRunComponent(_api, _toastr) {
        this._api = _api;
        this._toastr = _toastr;
        this.OnAdded = new core_1.EventEmitter();
    }
    BotRunComponent.prototype.ngOnInit = function () {
        var _this = this;
        this._api.GetAccounts().subscribe(function (response) { return _this.Accounts = response; });
        this._api.GetPackages().subscribe(function (response) { return _this.Packages = response; });
    };
    BotRunComponent.prototype.addBot = function () {
        var _this = this;
        console.info(this.Setup.Payload);
        this._api.SetupPlugin(this.Setup).subscribe(function (tb) { return _this.OnAdded.emit(tb); }, function (err) {
            if (!err.Handled)
                _this._toastr.error(err.Message);
        });
    };
    BotRunComponent.prototype.cancel = function () {
    };
    BotRunComponent.prototype.onPluginSelected = function (plugin) {
        this.Setup = index_1.PluginSetupModel.Create(plugin);
    };
    return BotRunComponent;
}());
__decorate([
    core_1.Output(),
    __metadata("design:type", Object)
], BotRunComponent.prototype, "OnAdded", void 0);
BotRunComponent = __decorate([
    core_1.Component({
        selector: 'bot-run-cmp',
        template: require('./bot-run.component.html'),
        styles: [require('../../app.component.css')],
    }),
    __metadata("design:paramtypes", [index_2.ApiService, index_2.ToastrService])
], BotRunComponent);
exports.BotRunComponent = BotRunComponent;
//# sourceMappingURL=bot-run.component.js.map