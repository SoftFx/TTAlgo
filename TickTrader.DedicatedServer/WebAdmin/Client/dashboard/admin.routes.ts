import { Route } from '@angular/router';
import { AdminComponent } from './admin.component';
import { DashboardComponent } from './dashboard.component';
import { OverlayComponent } from './overlay.component';
import { RepositoryComponent, PackageCardComponent } from './repository/index';
import { AccountsComponent } from './accounts/accounts.component';
import { BotRunComponent } from './bot-run.component';
import { BotSettingsComponent } from './bot-settings.component';
import { BotDetailComponent } from './bot-detail.component';
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
    RepositoryComponent,
    PackageCardComponent,
    BotDetailComponent,
    BotRunComponent,
    BotSettingsComponent,
    OverlayComponent
]
