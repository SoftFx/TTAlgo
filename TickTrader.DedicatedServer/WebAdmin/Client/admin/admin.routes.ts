import { Route } from '@angular/router';
import { AdminComponent } from './admin.component';
import { DashboardComponent, BotSetupComponent, BotAddComponent, BotParametersComponent, BotDetailComponent, BotConfigurationComponent, BotCardComponent } from './dashboard/index';
import { RepositoryComponent, PackageCardComponent } from './repository/index';
import { AccountsComponent, AccountCardComponent, ChangePasswordComponent, TestAccountComponent } from './accounts/index';
import { AuthGuard } from '../services/index';

export const MODULE_ROUTES: Route[] = [
    {
        path: '', canActivate: [AuthGuard], component: AdminComponent,
        children: [
            { path: 'dashboard', canActivate: [AuthGuard], component: DashboardComponent },
            { path: 'repository', canActivate: [AuthGuard], component: RepositoryComponent },
            { path: 'accounts', canActivate: [AuthGuard], component: AccountsComponent },
            { path: 'bot/:id', canActivate: [AuthGuard], component: BotDetailComponent },
            { path: 'configurate/:id', canActivate: [AuthGuard], component: BotConfigurationComponent },
            { path: 'configurate', canActivate: [AuthGuard], component: BotConfigurationComponent },
            { path: '', redirectTo: 'dashboard', pathMatch: "full" }
        ]
    }
]

export const MODULE_COMPONENTS = [
    AdminComponent,
    DashboardComponent,
    AccountsComponent,
    AccountCardComponent,
    ChangePasswordComponent,
    TestAccountComponent,
    RepositoryComponent,
    PackageCardComponent,
    BotDetailComponent,
    BotConfigurationComponent,
    BotSetupComponent,
    BotAddComponent,
    BotParametersComponent,
    BotCardComponent
]
