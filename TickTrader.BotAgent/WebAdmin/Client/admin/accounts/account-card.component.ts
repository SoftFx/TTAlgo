import { Input, EventEmitter, Output, Component } from '@angular/core';
import { AccountModel, ConnectionErrorCodes, ConnectionTestResult, ObservableRequest } from '../../models/index';
import { ApiService, ToastrService } from '../../services/index';

@Component({
    selector: 'account-card-cmp',
    template: require('./account-card.component.html'),
    styles: [require('../../app.component.css')],
})

export class AccountCardComponent {

    public ChangePasswordEnabled: boolean;
    public TestAccountEnabled: boolean;
    public ConfirmDeletionEnabled: boolean;

    constructor(private _api: ApiService, private _toastr: ToastrService) { }

    @Input() Account: AccountModel;
    @Output() OnDeleted = new EventEmitter<AccountModel>();
    @Output() OnUpdated = new EventEmitter<AccountModel>();

    public InitChangePassword() {
        this.ChangePasswordEnabled = true;
    }

    public InitTestAccount() {
        this.TestAccountEnabled = true;
    }

    public InitDeletion() {
        this.ConfirmDeletionEnabled = true;
    }

    public DeletionCanceled() {
        this.ConfirmDeletionEnabled = false;
    }

    public DeletionCompleted(account: AccountModel) {
        this.OnDeleted.emit(account);
    }

    public PasswordChangedOrCnaceled() {
        this.ChangePasswordEnabled = false;
    }

    public TestCanceled() {
        this.TestAccountEnabled = false;
    }
}