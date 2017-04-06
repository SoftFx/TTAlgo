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
    public statusMessage: string;
    public creds: AuthCredentials = new AuthCredentials('Administrator', 'Administrator');

    constructor(
        private router: Router,
        private _api: ApiService) {
    }

    login() {
        this._api.Auth.LogIn(this.creds.login, this.creds.password)
            .subscribe(
            data => {
                let redirectUrl = this._api.Auth.RedirectUrl ? this._api.Auth.RedirectUrl : '/';
                this.router.navigate([redirectUrl]);
            });
    }
}
