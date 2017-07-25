import { Component, OnInit } from '@angular/core';
import { MenuType, RouteInfo } from '../../models/index';
import { AuthService, NavigationService } from '../../services/index';

@Component({
    selector: 'sidebar-cmp',
    template: require('./sidebar.component.html')
})

export class SidebarComponent implements OnInit {
    public SideMenuItems: any[];

    constructor(private navService: NavigationService) { }

    ngOnInit() {
        this.SideMenuItems = this.navService.SideMenuItems();
    }

    public get TopMenuItems(): any {
        return this.navService.TopMenuItems();
    }

    IsMobileMenu() {
        return this.navService.IsMobileVersion();
    }
}
