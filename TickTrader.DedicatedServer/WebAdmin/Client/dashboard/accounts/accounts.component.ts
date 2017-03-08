import { OnInit, Component } from '@angular/core';
import { ApiService } from '../../services/api.service';
import { AccountModel } from '../../models/index';

@Component({
    selector: 'accounts-cmp',
    template: require('./accounts.component.html'),
    styles: [require('../../app.component.css')]
})

export class AccountsComponent implements OnInit {

    public Accounts: AccountModel[] = [];
    public Account: AccountModel = new AccountModel();

    constructor(private Api: ApiService) {
        this.Accounts.push(new AccountModel());
        this.Accounts.push(new AccountModel());
        this.Accounts.push(new AccountModel());
        this.Accounts.push(new AccountModel());
        this.Accounts.push(new AccountModel());
    }

    ngOnInit() {
       
    }

    public Add() { }

    public Cancel() { }

    public Test() { }
}