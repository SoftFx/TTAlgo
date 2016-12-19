import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/index';


@Component({
    selector: 'nav-menu',
    template: require('./navmenu.component.html'),
    styles: [require('./navmenu.component.css')]
})
export class NavMenuComponent {
    public authService: AuthService;

    constructor(private router: Router, authService: AuthService) {
        this.authService = authService;
    }

    logOut() {
        this.authService.logOut();
        this.router.navigate(['/login']);
    }

}
