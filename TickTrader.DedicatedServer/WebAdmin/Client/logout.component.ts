import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from './services/index';
import { AuthCredentials } from './models/index';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';

@Component({
    selector: 'logout',
    template: require('./logout.component.html')
})

export class LogoutComponent {
    public LogInStatusMessage: string;
    public Creds: AuthCredentials = new AuthCredentials();

    constructor(private _api: ApiService) {
    }

    ngOnInit() {
        this._api.Auth.LogOut();
    }
}
