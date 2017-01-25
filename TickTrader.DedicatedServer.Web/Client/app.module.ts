import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { UniversalModule } from 'angular2-universal';
import {
    AppComponent,
    BotsRepositoryComponent,
    BotAddComponent, BotDetailComponent, DashboardComponent,
    LoginComponent,
    NavMenuComponent
} from './components/index'
import { AuthService, AuthGuard, ApiService } from './services/index';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { OrderBy, FilterByPipe } from './pipes/index';

@NgModule({
    bootstrap: [ AppComponent ],
    declarations: [
        AppComponent,
        NavMenuComponent,
        BotsRepositoryComponent,
        DashboardComponent,
        BotDetailComponent,
        BotAddComponent,
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
        ReactiveFormsModule,
        RouterModule.forRoot([
            { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard] },
            { path: 'bot-detail/:id', component: BotDetailComponent, canActivate: [AuthGuard] },
            { path: 'bot-add', component: BotAddComponent, canActivate: [AuthGuard] },
            { path: 'login', component: LoginComponent },
            { path: 'bots-repository', component: BotsRepositoryComponent, canActivate: [AuthGuard] },
            { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
            { path: '**', redirectTo: 'dashboard' }
        ])
    ]
})
export class AppModule {
}
