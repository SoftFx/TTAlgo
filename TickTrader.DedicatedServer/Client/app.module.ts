import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { UniversalModule } from 'angular2-universal';
import { AppComponent } from './components/app/app.component'
import { NavMenuComponent } from './components/navmenu/navmenu.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { LoginComponent } from './components/login/login.component';
import { BotsRepositoryComponent } from './components/bots-repository/bots-repository.component';
import { AuthService, AuthGuard, ApiService } from './services/index';
import { FormsModule } from '@angular/forms';
import { OrderBy, FilterByPipe } from './pipes/index';

@NgModule({
    bootstrap: [ AppComponent ],
    declarations: [
        AppComponent,
        NavMenuComponent,
        BotsRepositoryComponent,
        DashboardComponent,
        LoginComponent,
        OrderBy,
        FilterByPipe
    ],
    providers: [
        AuthService,
        AuthGuard,
        ApiService
    ],
    imports: [
        UniversalModule, // Must be first import. This automatically imports BrowserModule, HttpModule, and JsonpModule too.
        FormsModule,
        RouterModule.forRoot([
            { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
            { path: 'login', component: LoginComponent },
            { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard] },
            { path: 'bots-repository', component: BotsRepositoryComponent, canActivate: [AuthGuard] },
            { path: '**', redirectTo: 'dashboard' }
        ])
    ]
})
export class AppModule {
}
