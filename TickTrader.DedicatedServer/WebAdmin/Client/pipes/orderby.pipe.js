"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var core_1 = require("@angular/core");
var OrderByPipe = OrderByPipe_1 = (function () {
    function OrderByPipe() {
    }
    OrderByPipe._orderByComparator = function (a, b) {
        if (OrderByPipe_1.isBoolean(a) && OrderByPipe_1.isBoolean(b)) {
            a = +a;
            b = +b;
        }
        if ((isNaN(parseFloat(a)) || !isFinite(a)) || (isNaN(parseFloat(b)) || !isFinite(b))) {
            if (a.toLowerCase() < b.toLowerCase())
                return -1;
            if (a.toLowerCase() > b.toLowerCase())
                return 1;
        }
        else {
            if (parseFloat(a) < parseFloat(b))
                return -1;
            if (parseFloat(a) > parseFloat(b))
                return 1;
        }
        return 0;
    };
    OrderByPipe.prototype.transform = function (input, config) {
        if (config === void 0) { config = ['+']; }
        if (!input)
            return input;
        if (!Array.isArray(input))
            return input;
        if (!Array.isArray(config) || (Array.isArray(config) && config.length == 1)) {
            var propertyToCheck = Array.isArray(config) ? config[0] : config;
            var desc = propertyToCheck.substr(0, 1) == '-';
            if (!propertyToCheck || propertyToCheck == '-' || propertyToCheck == '+') {
                return !desc ? input.sort() : input.sort().reverse();
            }
            else {
                var property = propertyToCheck.substr(0, 1) == '+' || propertyToCheck.substr(0, 1) == '-'
                    ? propertyToCheck.substr(1)
                    : propertyToCheck;
                return input.sort(function (a, b) {
                    return !desc ? OrderByPipe_1._orderByComparator(a[property], b[property])
                        : -OrderByPipe_1._orderByComparator(a[property], b[property]);
                });
            }
        }
        else {
            return input.sort(function (a, b) {
                for (var i = 0; i < config.length; i++) {
                    var desc = config[i].substr(0, 1) == '-';
                    var property = config[i].substr(0, 1) == '+' || config[i].substr(0, 1) == '-'
                        ? config[i].substr(1)
                        : config[i];
                    var comparison = !desc ? OrderByPipe_1._orderByComparator(a[property], b[property])
                        : -OrderByPipe_1._orderByComparator(a[property], b[property]);
                    if (comparison != 0)
                        return comparison;
                }
                return 0;
            });
        }
    };
    OrderByPipe.isBoolean = function (obj) {
        var type = typeof obj;
        return type === "boolean";
    };
    return OrderByPipe;
}());
OrderByPipe = OrderByPipe_1 = __decorate([
    core_1.Pipe({
        name: 'orderBy',
        pure: false
    })
], OrderByPipe);
exports.OrderByPipe = OrderByPipe;
var OrderByPipe_1;
//# sourceMappingURL=orderby.pipe.js.map