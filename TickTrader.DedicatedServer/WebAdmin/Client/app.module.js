"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var core_1 = require("@angular/core");
var router_1 = require("@angular/router");
var angular2_universal_1 = require("angular2-universal");
var index_1 = require("./services/index");
var forms_1 = require("@angular/forms");
var expression_true_directive_1 = require("./directives/expression-true.directive");
var app_component_1 = require("./app.component");
var login_component_1 = require("./login.component");
var admin_module_1 = require("./admin/admin.module");
var footer_module_1 = require("./shared/footer/footer.module");
var AppModule = (function () {
    function AppModule() {
    }
    return AppModule;
}());
AppModule = __decorate([
    core_1.NgModule({
        bootstrap: [app_component_1.AppComponent],
        declarations: [
            login_component_1.LoginComponent,
            app_component_1.AppComponent,
            expression_true_directive_1.ExpressionTrue
        ],
        providers: [
            index_1.AuthService,
            index_1.FeedService,
            index_1.AuthGuard,
            index_1.ApiService,
            index_1.ResourceService,
            index_1.ToastrService
        ],
        imports: [
            angular2_universal_1.UniversalModule,
            footer_module_1.FooterModule,
            forms_1.FormsModule,
            admin_module_1.AdminModule,
            forms_1.ReactiveFormsModule,
            router_1.RouterModule.forRoot([
                { path: 'login', component: login_component_1.LoginComponent },
                { path: '', redirectTo: 'dashboard', pathMatch: "full" },
                { path: '**', redirectTo: 'dashboard' }
            ])
        ]
    })
], AppModule);
exports.AppModule = AppModule;
//# sourceMappingURL=app.module.js.map