import { Injectable } from '@angular/core';
import { MenuType, RouteInfo, Action } from '../models/index';

@Injectable()
export class NavigationService {

    SideBarOpened: boolean;

    Routes: RouteInfo[] = [
        { path: '/dashboard', title: 'Dashboard', menuType: MenuType.SideBar, icon: 'fa fa-tachometer', owner: null },
        { path: '/configurate', title: 'Add Bot', menuType: MenuType.NavBar, icon: '', owner: "/dashboard" },
        { path: '/repository', title: 'Repository', menuType: MenuType.SideBar, icon: 'fa fa-cloud', owner: null },
        { path: '/accounts', title: 'Accounts', menuType: MenuType.SideBar, icon: 'fa fa-users', owner: null },
        { path: '/logout', title: 'Log Out', menuType: MenuType.NavBar, icon: 'fa fa-sign-out', owner: null },
    ];

    TopMenuItems(): RouteInfo[]{
        var titlee = window.location.pathname;

        return this.Routes.filter(menuItem => menuItem.menuType === MenuType.NavBar && menuItem.owner === titlee)
            .concat(this.Routes.filter(menuItem => menuItem.menuType === MenuType.NavBar && menuItem.owner == null));
    }

    SideMenuItems(): RouteInfo[] {
        return this.Routes.filter(menuItem => menuItem.menuType === MenuType.SideBar);
    }

    ToggleSideBar(): void {
        this.SideBarOpened = !this.SideBarOpened;
    }

    IsMobileVersion() {
        if ($(window).width() > 991) {
            return false;
        }
        return true;
    }
}