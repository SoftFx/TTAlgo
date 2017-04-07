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
var forms_1 = require("@angular/forms");
var ExpressionTrue = ExpressionTrue_1 = (function () {
    function ExpressionTrue() {
    }
    ExpressionTrue.prototype.validate = function (control) {
        console.log('ExpressionTrue!!');
        if (this.predicate != null && !this.predicate()) {
            return { 'expressionTrue': false };
        }
        return null;
    };
    return ExpressionTrue;
}());
__decorate([
    core_1.Input('expressionTrue'),
    __metadata("design:type", Function)
], ExpressionTrue.prototype, "predicate", void 0);
ExpressionTrue = ExpressionTrue_1 = __decorate([
    core_1.Directive({
        selector: '[expressionTrue]',
        providers: [{ provide: forms_1.NG_VALIDATORS, useExisting: ExpressionTrue_1, multi: true }]
    })
], ExpressionTrue);
exports.ExpressionTrue = ExpressionTrue;
var ExpressionTrue_1;
//# sourceMappingURL=expression-true.directive.js.map