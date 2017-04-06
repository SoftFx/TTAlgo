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
var PackageCardComponent = (function () {
    function PackageCardComponent(_api, _toastr) {
        this._api = _api;
        this._toastr = _toastr;
        this.OnDeleted = new core_1.EventEmitter();
    }
    PackageCardComponent.prototype.Delete = function () {
        var _this = this;
        this._api
            .DeletePackage(this.Package.Name)
            .subscribe(function () { return _this.OnDeleted.emit(_this.Package); }, function (err) { return _this._toastr.error("Error deleting package " + _this.Package.Name); });
    };
    return PackageCardComponent;
}());
__decorate([
    core_1.Input(),
    __metadata("design:type", index_1.PackageModel)
], PackageCardComponent.prototype, "Package", void 0);
__decorate([
    core_1.Output(),
    __metadata("design:type", Object)
], PackageCardComponent.prototype, "OnDeleted", void 0);
PackageCardComponent = __decorate([
    core_1.Component({
        selector: 'package-card-cmp',
        template: require('./package-card.component.html'),
        styles: [require('../../app.component.css')],
    }),
    __metadata("design:paramtypes", [index_2.ApiService, index_2.ToastrService])
], PackageCardComponent);
exports.PackageCardComponent = PackageCardComponent;
//# sourceMappingURL=package-card.component.js.map