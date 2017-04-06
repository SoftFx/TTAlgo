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
var core_2 = require("@angular/core");
var RepositoryComponent = (function () {
    function RepositoryComponent(_api, _toastr) {
        this._api = _api;
        this._toastr = _toastr;
        this._isFileDuplicated = false;
        this.Packages = [];
        this.Uploading = false;
    }
    RepositoryComponent.prototype.ngOnInit = function () {
        var _this = this;
        this._api.Feed.addPackage.subscribe(function (algoPackage) { return _this.addPackage(algoPackage); });
        this._api.Feed.deletePackage.subscribe(function (pname) { return _this.deletePackage(pname); });
        this.loadPackages();
    };
    Object.defineProperty(RepositoryComponent.prototype, "SelectedFileName", {
        get: function () {
            if (this.SelectedFile)
                return this.SelectedFile.name;
            else
                return "";
        },
        enumerable: true,
        configurable: true
    });
    RepositoryComponent.prototype.OnPackageDeleted = function (algoPackage) {
        //this.deletePackage(algoPackage.Name);
    };
    RepositoryComponent.prototype.UploadPackage = function () {
        var _this = this;
        this._uploadingError = null;
        this.Uploading = true;
        this._api
            .UploadPackage(this.SelectedFile)
            .finally(function () { _this.Uploading = false; })
            .subscribe(function (res) {
            _this.SelectedFile = null;
            _this.PackageInput.nativeElement.value = "";
        }, function (err) {
            _this._uploadingError = err;
            if (!_this._uploadingError.Handled)
                _this._toastr.error(_this._uploadingError.Message);
        });
    };
    RepositoryComponent.prototype.OnFileInputChange = function (event) {
        this._uploadingError = null;
        this.SelectedFile = event.target.files[0];
    };
    Object.defineProperty(RepositoryComponent.prototype, "FileInputError", {
        get: function () {
            if ((this._uploadingError != null && this._uploadingError['Code'] && this._uploadingError.Code == 1000) || this.isFileDuplicated) {
                return 'DuplicatePackage';
            }
            return null;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(RepositoryComponent.prototype, "IsFileNameVaild", {
        get: function () {
            var a = this.SelectedFile && !this.SelectedFile.name && !this.isFileDuplicated;
            return a;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(RepositoryComponent.prototype, "CanUpload", {
        get: function () {
            return !this.Uploading
                && this.SelectedFileName
                && !this.isFileDuplicated
                && (!this._uploadingError || !this._uploadingError['Code'] || this._uploadingError['Code'] != 1000);
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(RepositoryComponent.prototype, "isFileDuplicated", {
        get: function () {
            var _this = this;
            return this.Packages.find(function (p) { return _this.SelectedFile && p.Name == _this.SelectedFile.name; }) != null;
        },
        enumerable: true,
        configurable: true
    });
    RepositoryComponent.prototype.loadPackages = function () {
        var _this = this;
        this._api.GetPackages()
            .subscribe(function (res) { return _this.Packages = res; });
    };
    RepositoryComponent.prototype.addPackage = function (packageModel) {
        if (!this.Packages.find(function (p) { return p.Name === packageModel.Name; })) {
            this.Packages = this.Packages.concat(packageModel);
        }
    };
    RepositoryComponent.prototype.deletePackage = function (packageName) {
        this.Packages = this.Packages.filter(function (p) { return p.Name !== packageName; });
    };
    return RepositoryComponent;
}());
__decorate([
    core_2.ViewChild('PackageInput'),
    __metadata("design:type", Object)
], RepositoryComponent.prototype, "PackageInput", void 0);
RepositoryComponent = __decorate([
    core_1.Component({
        selector: 'repository-cmp',
        template: require('./repository.component.html'),
        styles: [require('../../app.component.css')]
    }),
    __metadata("design:paramtypes", [index_1.ApiService, index_1.ToastrService])
], RepositoryComponent);
exports.RepositoryComponent = RepositoryComponent;
//# sourceMappingURL=repository.component.js.map