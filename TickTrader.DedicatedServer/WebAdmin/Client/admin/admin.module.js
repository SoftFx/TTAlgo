"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var core_1 = require("@angular/core");
var router_1 = require("@angular/router");
var angular2_masonry_1 = require("angular2-masonry");
var index_1 = require("../pipes/index");
var base64_file_input_directive_1 = require("../directives/base64-file-input.directive");
var angular2_universal_1 = require("angular2-universal");
var forms_1 = require("@angular/forms");
var navbar_module_1 = require("../shared/navbar/navbar.module");
var sidebar_module_1 = require("../shared/sidebar/sidebar.module");
var footer_module_1 = require("../shared/footer/footer.module");
var overlay_component_1 = require("../shared/overlay.component");
var admin_routes_1 = require("./admin.routes");
var AdminModule = (function () {
    function AdminModule() {
    }
    return AdminModule;
}());
AdminModule = __decorate([
    core_1.NgModule({
        imports: [
            angular2_universal_1.UniversalModule,
            angular2_masonry_1.MasonryModule,
            forms_1.FormsModule,
            forms_1.ReactiveFormsModule,
            navbar_module_1.NavbarModule,
            sidebar_module_1.SidebarModule,
            footer_module_1.FooterModule,
            router_1.RouterModule.forChild(admin_routes_1.MODULE_ROUTES)
        ],
        declarations: [
            index_1.OrderByPipe,
            index_1.FilterByPipe,
            index_1.ResourcePipe,
            overlay_component_1.OverlayComponent,
            base64_file_input_directive_1.FileModelDirective,
            admin_routes_1.MODULE_COMPONENTS,
        ]
    })
], AdminModule);
exports.AdminModule = AdminModule;
//# sourceMappingURL=admin.module.js.map