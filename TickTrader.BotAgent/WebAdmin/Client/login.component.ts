import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from './services/index';
import { AuthCredentials, ResponseCode } from './models/index';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';

@Component({
    selector: 'login',
    template: require('./login.component.html'),
    styles: [require('./app.component.css'), require('./login.component.css')]
})

export class LoginComponent {
    public LogInStatusMessage: string;
    public Creds: AuthCredentials = new AuthCredentials();
    public ShowForm: boolean;
    public ShowForm2FA: boolean;

    constructor(
        private router: Router,
        private _api: ApiService) {

        this.ShowForm = true;
        this.ShowForm2FA = false;
    }

    LogIn() {
        this.LogInStatusMessage = null;

        this._api.Auth.LogIn(this.Creds.Login, this.Creds.Password, this.Creds.SecretCode).subscribe(
            ok => {
                let redirectUrl = this._api.Auth.RedirectUrl ? this._api.Auth.RedirectUrl : '/';
                this.router.navigate([redirectUrl]);
            },
            err => {
                if (err.Code == ResponseCode.Requires2FA) {
                    this.ShowForm = false;
                    this.ShowForm2FA = true;
                }
                else {
                    this.LogInStatusMessage = err.Message;
                }
            }
        );
    }
}
