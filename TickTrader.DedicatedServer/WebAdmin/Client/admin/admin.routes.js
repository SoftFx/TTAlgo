"use strict";
var admin_component_1 = require("./admin.component");
var index_1 = require("./dashboard/index");
var index_2 = require("./repository/index");
var index_3 = require("./accounts/index");
var index_4 = require("../services/index");
exports.MODULE_ROUTES = [
    {
        path: '', canActivate: [index_4.AuthGuard], component: admin_component_1.AdminComponent,
        children: [
            { path: 'dashboard', canActivate: [index_4.AuthGuard], component: index_1.DashboardComponent },
            { path: 'repository', canActivate: [index_4.AuthGuard], component: index_2.RepositoryComponent },
            { path: 'accounts', canActivate: [index_4.AuthGuard], component: index_3.AccountsComponent },
            { path: 'bot/:id', canActivate: [index_4.AuthGuard], component: index_1.BotDetailComponent },
            { path: '', redirectTo: 'dashboard', pathMatch: "full" }
        ]
    }
];
exports.MODULE_COMPONENTS = [
    admin_component_1.AdminComponent,
    index_1.DashboardComponent,
    index_3.AccountsComponent,
    index_3.AccountCardComponent,
    index_2.RepositoryComponent,
    index_2.PackageCardComponent,
    index_1.BotDetailComponent,
    index_1.BotRunComponent,
    index_1.PluginSetupComponent
];
//# sourceMappingURL=admin.routes.js.map