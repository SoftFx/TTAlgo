import { Route } from '@angular/router';
import { AdminComponent } from './admin.component';
import { DashboardComponent, BotRunComponent, PluginSetupComponent, BotDetailComponent } from './dashboard/index';
import { RepositoryComponent, PackageCardComponent } from './repository/index';
import { AccountsComponent, AccountCardComponent } from './accounts/index';
import { AuthGuard } from '../services/index';

export const MODULE_ROUTES: Route[] = [
    {
        path: '', canActivate: [AuthGuard], component: AdminComponent,
        children: [
            { path: 'dashboard', canActivate: [AuthGuard], component: DashboardComponent },
            { path: 'repository', canActivate: [AuthGuard], component: RepositoryComponent },
            { path: 'accounts', canActivate: [AuthGuard], component: AccountsComponent },
            { path: 'bot/:id', canActivate: [AuthGuard], component: BotDetailComponent },
            { path: '', redirectTo: 'dashboard', pathMatch: "full" }
        ]
    }
]

export const MODULE_COMPONENTS = [
    AdminComponent,
    DashboardComponent,
    AccountsComponent,
    AccountCardComponent,
    RepositoryComponent,
    PackageCardComponent,
    BotDetailComponent,
    BotRunComponent,
    PluginSetupComponent
]
