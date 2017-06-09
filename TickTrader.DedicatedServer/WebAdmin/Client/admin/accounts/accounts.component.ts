import { OnInit, Component } from '@angular/core';
import { ApiService, ToastrService } from '../../services/index';
import { AccountModel, ResponseStatus, ResponseCode, ConnectionErrorCodes, ConnectionTestResult } from '../../models/index';

@Component({
    selector: 'accounts-cmp',
    template: require('./accounts.component.html'),
    styles: [require('../../app.component.css')]
})

export class AccountsComponent implements OnInit {
    public Accounts: AccountModel[];

    constructor(private _api: ApiService) {
    }

    ngOnInit() {
        this.Accounts = [];

        this._api.Feed.AddAccount.subscribe(acc => this.addAccount(acc));
        this._api.Feed.DeleteAccount.subscribe(acc => this.deleteAccount(acc));

        this._api.GetAccounts()
            .subscribe(result => {
                if (!this.Accounts)
                    this.Accounts = result;
                else
                    result.forEach(acc => this.addAccount(acc))
            });
    }

    public Delete(account: AccountModel) {
        this.deleteAccount(account);
    }

    public Add(account: AccountModel) {
        this.addAccount(account);
    }

    private addAccount(account: AccountModel) {
        if (!this.Accounts.find(a => a.Login === account.Login && a.Server === account.Server)) {
            this.Accounts = this.Accounts.concat(account);
        }
    }

    private deleteAccount(account: AccountModel) {
        this.Accounts = this.Accounts.filter(a => !(a.Login === account.Login && a.Server === account.Server));
    }
}