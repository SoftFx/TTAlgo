import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from './services/index';
import { AuthCredentials } from './models/index';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';

@Component({
    selector: 'login',
    template: require('./login.component.html'),
    styles: [require('./app.component.css'), require('./login.component.css')]
})

export class LoginComponent {
    public LogInStatusMessage: string;
    public Creds: AuthCredentials = new AuthCredentials();

    constructor(
        private router: Router,
        private _api: ApiService) {
    }

    LogIn() {
        this._api.Auth.LogIn(this.Creds.Login, this.Creds.Password).subscribe(
            ok => {
                let redirectUrl = this._api.Auth.RedirectUrl ? this._api.Auth.RedirectUrl : '/';
                this.router.navigate([redirectUrl]);
            },
            err => this.LogInStatusMessage = err.Message
        );
    }
}
