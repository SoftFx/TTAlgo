import { Route } from '@angular/router';
import { AdminComponent } from './admin.component';
import { DashboardComponent, BotSetupComponent, BotAddComponent, BotDeleteComponent, BotParametersComponent, BotDetailsComponent, BotConfigurationComponent, BotCardComponent } from './dashboard/index';
import { RepositoryComponent, PackageCardComponent, PacakgeDeleteComponent } from './repository/index';
import { AccountsComponent, AccountCardComponent, AccountChangePasswordComponent, AccountTestComponent, AccountDeleteComponent, AccountAddComponent } from './accounts/index';
import { AuthGuard } from '../services/index';

export const MODULE_ROUTES: Route[] = [
    {
        path: '', canActivate: [AuthGuard], component: AdminComponent,
        children: [
            { path: 'bot/:id', canActivate: [AuthGuard], component: BotDetailsComponent },
            { path: 'configurate/:id', canActivate: [AuthGuard], component: BotConfigurationComponent },
            { path: 'configurate', canActivate: [AuthGuard], component: BotConfigurationComponent },
            { path: 'dashboard', canActivate: [AuthGuard], component: DashboardComponent },
            { path: 'repository', canActivate: [AuthGuard], component: RepositoryComponent },
            { path: 'accounts', canActivate: [AuthGuard], component: AccountsComponent },
            { path: '', redirectTo: 'dashboard', pathMatch: "full" }
        ]
    }
]

export const MODULE_COMPONENTS = [
    AdminComponent,
    DashboardComponent,
    AccountsComponent,
    AccountCardComponent,
    AccountChangePasswordComponent,
    AccountTestComponent,
    AccountDeleteComponent,
    AccountAddComponent,
    RepositoryComponent,
    PackageCardComponent,
    PacakgeDeleteComponent,
    BotDetailsComponent,
    BotConfigurationComponent,
    BotSetupComponent,
    BotAddComponent,
    BotDeleteComponent,
    BotParametersComponent,
    BotCardComponent
]
