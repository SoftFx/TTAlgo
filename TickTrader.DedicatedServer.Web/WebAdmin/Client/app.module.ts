import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { UniversalModule } from 'angular2-universal';
import { AuthService, AuthGuard, ApiService } from './services/index';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { AppComponent } from './app.component';
import { LoginComponent } from './login.component';

import { AdminModule } from './dashboard/admin.module';
import { FooterModule } from './shared/footer/footer.module';


@NgModule({
    bootstrap: [ AppComponent ],
    declarations: [
        AppComponent,
        LoginComponent,
    ],
    providers: [
        AuthService,
        AuthGuard,
        ApiService
    ],
    imports: [
        UniversalModule, // Must be first import. This automatically imports BrowserModule, HttpModule, and JsonpModule too.
        FooterModule,
        FormsModule,
        AdminModule,
        ReactiveFormsModule,
        RouterModule.forRoot([
            { path: 'login', component: LoginComponent },
            { path: '', redirectTo: 'dashboard', pathMatch: "full" },
            { path: '**', redirectTo: 'dashboard' }
        ])
    ]
})
export class AppModule {

}
