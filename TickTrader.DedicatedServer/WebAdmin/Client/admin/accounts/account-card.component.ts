import { Input, EventEmitter, Output, Component } from '@angular/core';
import { AccountModel, ConnectionErrorCodes, ConnectionTestResult } from '../../models/index';
import { ApiService, ToastrService } from '../../services/index';

@Component({
    selector: 'account-card-cmp',
    template: require('./account-card.component.html'),
    styles: [require('../../app.component.css')],
})

export class AccountCardComponent {

    public ChangePasswordEnabled: boolean;
    public TestAccountEnabled: boolean;

    constructor(private _api: ApiService, private _toastr: ToastrService) { }

    @Input() Account: AccountModel;
    @Output() onDeleted = new EventEmitter<AccountModel>();
    @Output() onUpdated = new EventEmitter<AccountModel>();

    /* Change Password */
    public ChangePassword() {
        this.ChangePasswordEnabled = true;
    }

    public PasswordChangedOrCnaceled() {
        this.ChangePasswordEnabled = false;
    }

    /*Test Account*/
    public TestAccount() {
        this.TestAccountEnabled = true;
    }

    public TestAccountClosed() {
        this.TestAccountEnabled = false;
    }

    public Delete() {
        this._api.DeleteAccount(Object.assign(new AccountModel(), this.Account))
            .subscribe(ok => this.onDeleted.emit(this.Account),
            err => this._toastr.error(`Error deleting account ${this.Account.Login} (${this.Account.Server})`));
    }
}