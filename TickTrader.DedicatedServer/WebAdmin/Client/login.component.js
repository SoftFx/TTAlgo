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
var router_1 = require("@angular/router");
var index_1 = require("./services/index");
var index_2 = require("./models/index");
var LoginComponent = (function () {
    function LoginComponent(router, authService) {
        this.router = router;
        this.authService = authService;
        this.creds = new index_2.AuthCredentials('Administrator', 'BestPasswordInTheWorld');
    }
    LoginComponent.prototype.login = function () {
        var _this = this;
        this.authService.logIn(this.creds.login, this.creds.password)
            .subscribe(function (data) {
            var redirectUrl = _this.authService.redirectUrl ? _this.authService.redirectUrl : '/';
            _this.router.navigate([redirectUrl]);
        });
    };
    return LoginComponent;
}());
LoginComponent = __decorate([
    core_1.Component({
        selector: 'login',
        template: require('./login.component.html'),
        styles: [require('./app.component.css'), require('./login.component.css')]
    }),
    __metadata("design:paramtypes", [router_1.Router,
        index_1.AuthService])
], LoginComponent);
exports.LoginComponent = LoginComponent;
//# sourceMappingURL=login.component.js.map