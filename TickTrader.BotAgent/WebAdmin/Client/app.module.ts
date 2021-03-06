import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { UniversalModule } from 'angular2-universal';
import { AuthService, FeedService, AuthGuard, ApiService, ResourceService, ToastrService, NavigationService } from './services/index';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ExpressionTrue } from './directives/expression-true.directive';

import { AppComponent } from './app.component';
import { LoginComponent } from './login.component';
import { LogoutComponent } from './logout.component';

import { AdminModule } from './admin/admin.module';
import { FooterModule } from './shared/footer/footer.module';

@NgModule({
    bootstrap: [ AppComponent ],
    declarations: [
        LoginComponent,
        LogoutComponent,
        AppComponent,
        ExpressionTrue
    ],
    providers: [
        AuthService,
        FeedService,
        AuthGuard,
        ApiService,
        ResourceService,
        ToastrService,
        NavigationService
    ],
    imports: [
        UniversalModule, // Must be first import. This automatically imports BrowserModule, HttpModule, and JsonpModule too.
        FooterModule,
        FormsModule,
        AdminModule,
        ReactiveFormsModule,
        RouterModule.forRoot([
            { path: 'login', component: LoginComponent },
            { path: 'logout', component: LogoutComponent },
            { path: '', redirectTo: 'dashboard', pathMatch: "full" },
            { path: '**', redirectTo: 'dashboard' }
        ])
    ]
})
export class AppModule {

}
