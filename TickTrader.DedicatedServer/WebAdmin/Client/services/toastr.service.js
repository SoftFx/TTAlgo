"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var core_1 = require("@angular/core");
require("bootstrap-notify");
var ToastrService = (function () {
    function ToastrService() {
        this.toastSettings = {
            type: "",
            timer: 1500,
            allow_dismiss: true,
            newest_on_top: true,
            animate: {
                enter: 'animated fadeInDown',
                exit: 'animated fadeOutUp'
            },
            icon_type: 'class',
            placement: {
                from: 'top',
                align: 'right'
            }
        };
    }
    ToastrService.prototype.info = function (message) {
        this.notify(message, "info");
    };
    ToastrService.prototype.success = function (message) {
        this.notify(message, "success");
    };
    ToastrService.prototype.warning = function (message) {
        this.notify(message, "warning");
    };
    ToastrService.prototype.error = function (message) {
        this.notify(message, "danger");
    };
    ToastrService.prototype.notify = function (message, type) {
        var localToastSettings = Object.assign({}, this.toastSettings);
        localToastSettings.type = type;
        $.notify({
            message: message
        }, localToastSettings);
    };
    return ToastrService;
}());
ToastrService = __decorate([
    core_1.Injectable()
], ToastrService);
exports.ToastrService = ToastrService;
var ToastrTypes;
(function (ToastrTypes) {
    ToastrTypes[ToastrTypes["info"] = 0] = "info";
    ToastrTypes[ToastrTypes["success"] = 1] = "success";
    ToastrTypes[ToastrTypes["warning"] = 2] = "warning";
    ToastrTypes[ToastrTypes["danger"] = 3] = "danger";
})(ToastrTypes || (ToastrTypes = {}));
//template:
//'<div data-notify="container" class="col-xs-11 col-sm-3 alert alert-{0}" role="alert">' +
//'   <button type="button" aria-hidden="true" class="close" data-notify="dismiss">×</button>' +
//'   <span data-notify="message">{2}</span>' +
//'</div>' 
//# sourceMappingURL=toastr.service.js.map