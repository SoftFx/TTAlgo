import { Component, OnInit } from '@angular/core';
import { AuthService, NavigationService } from '../services/index';
import { Router } from '@angular/router';

@Component({
    selector: 'admin-cmp',
    template: require('./admin.component.html'),
    styles: [require('../app.component.css')],
})

export class AdminComponent implements OnInit {

    constructor(private _router: Router, private _authService: AuthService, public navBar: NavigationService) { }

    ngOnInit() {
        $.getScript('/assets/js/gui.js');

        this._authService.AuthDataUpdated.subscribe(authData => {
            if (!authData) {
                this._router.navigate(['/login']);
            }
        });

        this.manageTokenExpiration();
    }

    private manageTokenExpiration(): void {
        setTimeout(() => {
            if (!this._authService.IsAuthorized) {
                this._authService.LogOut();
                this._router.navigate(['/login']);
            }
            else {
                this.manageTokenExpiration();
            }
        }, 10000);
    }
}
