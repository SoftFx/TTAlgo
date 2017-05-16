import { Component, OnInit } from '@angular/core';
import { ROUTES } from '../sidebar/sidebar-routes.config';
import { MenuType } from '../sidebar/sidebar.metadata';
import { AuthService } from '../../services/index';
import { Router } from '@angular/router';

@Component({
    selector: 'navbar-cmp',
    template: require('./navbar.component.html')
})

export class NavbarComponent implements OnInit {
    private listTitles: any[];

    constructor(private _router: Router, private _authService: AuthService) { }

    ngOnInit() {
        this.listTitles = ROUTES.filter(listTitle => listTitle.menuType !== MenuType.BRAND);
    }

    public get RouteInfo(): any {
        var titlee = window.location.pathname;
        for (var item = 0; item < this.listTitles.length; item++) {
            if (this.listTitles[item].path === titlee) {
                return this.listTitles[item];
            }
        }
        return this.listTitles[0];
    }

    public get UserName(): string {
        if (this._authService.IsAuthorized)
        {
            return this._authService.AuthData.User;
        }
        else
        {
            return '';
        }
    }

    public Logout() {
        this._authService.LogOut();
    }
}
