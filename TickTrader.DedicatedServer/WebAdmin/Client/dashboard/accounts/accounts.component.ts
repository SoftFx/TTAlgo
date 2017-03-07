import { OnInit, Component } from '@angular/core';
import { ApiService } from '../../services/api.service';
import { AccountModel } from '../../models/index';

@Component({
    selector: 'accounts-cmp',
    template: require('./accounts.component.html'),
    styles: [require('../../app.component.css')]
})

export class AccountsComponent {
    public accounts: AccountModel[];

    constructor(api: ApiService) { }

    
}