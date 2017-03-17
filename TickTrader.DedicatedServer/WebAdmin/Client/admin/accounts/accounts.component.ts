import { OnInit, Component } from '@angular/core';
import { ApiService, ToastrService } from '../../services/index';
import { AccountModel, ResponseStatus, ResponseCode, ConnectionErrorCodes, ConnectionTestResult } from '../../models/index';

@Component({
    selector: 'accounts-cmp',
    template: require('./accounts.component.html'),
    styles: [require('../../app.component.css')]
})

export class AccountsComponent implements OnInit {
    public TestResult: ConnectionTestResult;
    public ConnectionTestRunning: boolean;
    public Accounts: AccountModel[];
    public Account: AccountModel = new AccountModel();

    constructor(private _api: ApiService, private _toastr: ToastrService) {
    }

    ngOnInit() {
        this.Accounts = [];

        this._api.Feed.addAccount.subscribe(acc => { this.Accounts.push(acc); })
        this._api.Feed.deleteAccount.subscribe(acc => { this.Accounts = this.Accounts.filter(a => !(a.Login === acc.Login && a.Server === acc.Server)); })

        this._api.GetAccounts()
            .subscribe(result => this.Accounts = result);
    }

    public Add() {
        let accountClone = Object.assign(new AccountModel(), this.Account);
        this._api.AddAccount(accountClone)
            .subscribe(ok => this.Cancel(),
            err => {
                this._toastr.error(err.Message);
            });
    }

    public Cancel() {
        this.Account = new AccountModel();
    }

    public Test() {
        let accountClone = Object.assign(new AccountModel(), this.Account);
        this.ConnectionTestRunning = true;
        this.ResetTestResult();

        this._api.TestAccount(accountClone)
            .finally(() => { this.ConnectionTestRunning = false; })
            .subscribe(ok => this.TestResult = new ConnectionTestResult(ok.json()));
    }

    public ResetTestResult() {
        this.TestResult = null;
    }
}