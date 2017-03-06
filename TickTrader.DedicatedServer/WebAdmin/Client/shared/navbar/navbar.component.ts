import { Component, OnInit } from '@angular/core';
import { ROUTES } from '../../dashboard/sidebar/sidebar-routes.config';
import { MenuType } from '../../dashboard/sidebar/sidebar.metadata';
import { AuthService } from '../../services/index';
import { Router } from '@angular/router';

@Component({
    selector: 'navbar-cmp',
    template: require('./navbar.component.html')
})

export class NavbarComponent implements OnInit {
    private listTitles: any[];

    constructor(private router: Router, private authService: AuthService) { }

    ngOnInit() {
        this.listTitles = ROUTES.filter(listTitle => listTitle.menuType !== MenuType.BRAND);
    }

    getTitle() {
        var titlee = window.location.pathname;
        for (var item = 0; item < this.listTitles.length; item++) {
            if (this.listTitles[item].path === titlee) {
                return this.listTitles[item].title;
            }
        }
        return 'Dashboard';
    }

    public logout() {
        this.authService.logOut().subscribe(null, error => console.log('LogOut error: ' + error));
        this.router.navigate(['login']);
    }
}
