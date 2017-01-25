import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/index';
import { AuthCredentials } from '../../models/index';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';

@Component({
    selector: 'login',
    template: require('./login.component.html'),
    styles: [require('../app/app.component.css'), require('./login.component.css')]
})

export class LoginComponent {
    public statusMessage: string;
    public creds: AuthCredentials = new AuthCredentials('Administrator', 'BestPasswordInTheWorld');

    constructor(
        private router: Router,
        private authService: AuthService) {
    }

    login() {
        this.authService.logIn(this.creds.login, this.creds.password)
            .subscribe(
            data => {
                let redirectUrl = this.authService.redirectUrl ? this.authService.redirectUrl : '/';
                this.router.navigate([redirectUrl]);
            });
    }
}
