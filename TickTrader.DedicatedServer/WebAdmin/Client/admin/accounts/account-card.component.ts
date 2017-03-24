import { Input, EventEmitter, Output, Component } from '@angular/core';
import { AccountModel, ConnectionErrorCodes, ConnectionTestResult } from '../../models/index';
import { ApiService, ToastrService } from '../../services/index';

@Component({
    selector: 'account-card-cmp',
    template: require('./account-card.component.html'),
    styles: [require('../../app.component.css')],
})

export class AccountCardComponent {
    public TestResult: ConnectionTestResult;
    public ConnectionTestRunning: boolean;

    constructor(private _api: ApiService, private _toastr: ToastrService) { }

    @Input() Account: AccountModel;
    @Output() onDeleted = new EventEmitter<AccountModel>();
    @Output() onUpdated = new EventEmitter<AccountModel>();

    public Update() {
        this._api.UpdateAccount(<AccountModel>Object.assign({}, this.Account))
            .subscribe(ok => this.onUpdated.emit(this.Account),
            err => this._toastr.error(`Error updating account ${this.Account.Login} (${this.Account.Server})`));
    }

    public Delete() {
        this._api.DeleteAccount(Object.assign(new AccountModel(), this.Account))
            .subscribe(ok => this.onDeleted.emit(this.Account),
            err => this._toastr.error(`Error deleting account ${this.Account.Login} (${this.Account.Server})`));
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