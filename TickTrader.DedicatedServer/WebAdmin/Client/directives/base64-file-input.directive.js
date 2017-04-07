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
var FileModelDirective = (function () {
    function FileModelDirective() {
        var _this = this;
        this.fileModelChange = new core_1.EventEmitter();
        this._fileReader = new FileReader();
        this._fileReader.onload = function (file) {
            var base64Encoded = _this._fileReader.result;
            _this.file = { FileName: _this._file.name, Type: _this._file.type, Size: _this._file.size, Data: base64Encoded };
            _this.fileModelChange.emit(_this.file);
        };
    }
    FileModelDirective.prototype.onChange = function ($event) {
        this._file = $event.target.files[0];
        this.encodeFile(this._file);
    };
    FileModelDirective.prototype.encodeFile = function (file) {
        this._fileReader.readAsDataURL(file);
    };
    return FileModelDirective;
}());
__decorate([
    core_1.Input('fileModel'),
    __metadata("design:type", Object)
], FileModelDirective.prototype, "file", void 0);
__decorate([
    core_1.Output(),
    __metadata("design:type", core_1.EventEmitter)
], FileModelDirective.prototype, "fileModelChange", void 0);
FileModelDirective = __decorate([
    core_1.Directive({
        selector: '[fileModel]',
        host: {
            "(change)": 'onChange($event)'
        }
    }),
    __metadata("design:paramtypes", [])
], FileModelDirective);
exports.FileModelDirective = FileModelDirective;
//# sourceMappingURL=base64-file-input.directive.js.map