import { Component, OnInit } from '@angular/core';
import { MenuType, RouteInfo } from '../../models/index';
import { AuthService, NavigationService } from '../../services/index';
import { Router } from '@angular/router';

@Component({
    selector: 'navbar-cmp',
    template: require('./navbar.component.html')
})

export class NavbarComponent implements OnInit {
    private navBarItems: RouteInfo[];

    constructor(private _router: Router, private _authService: AuthService, private navService: NavigationService) { }

    ngOnInit() {
        this.navBarItems = this.navService.Routes.filter(item => item.menuType == MenuType.NavBar && item.owner == null);
    }

    public get MenuItems(): RouteInfo[] {

        return this.navService.TopMenuItems();
    }

    public get RouteInfo(): any {
        var path = window.location.pathname;
        return this.routeInfo(path);
    }

    private routeInfo(path: string): RouteInfo
    {
        for (var item = 0; item < this.navService.Routes.length; item++) {
            if (this.navService.Routes[item].path === path) {
                if (this.navService.Routes[item].owner == null)
                    return this.navService.Routes[item];
                else
                    return this.routeInfo(this.navService.Routes[item].owner);
            }
        }
        return this.navService.Routes[0];
    }
}
