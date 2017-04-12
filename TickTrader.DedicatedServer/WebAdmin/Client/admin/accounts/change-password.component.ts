import { Input, EventEmitter, Output, Component } from '@angular/core';
import { AccountModel, ConnectionErrorCodes, ConnectionTestResult } from '../../models/index';
import { ApiService, ToastrService } from '../../services/index';

@Component({
    selector: 'change-password-cmp',
    template: require('./change-password.component.html'),
    styles: [require('../../app.component.css')],
})

export class ChangePasswordComponent {
    public Password: string;

    constructor(private _api: ApiService, private _toastr: ToastrService) { }

    @Input() Account: AccountModel;
    @Output() OnChanged = new EventEmitter<void>();
    @Output() OnCanceled = new EventEmitter<void>();

    public ChangePassword() {
        let account = new AccountModel();
        account.Login = this.Account.Login;
        account.Server = this.Account.Server;
        account.Password = this.Password;

        this._api.ChangeAccountPassword(account)
            .subscribe(ok => this.OnChanged.emit(),
            err => this._toastr.error(`Error changing account password ${account.Login} (${account.Server})`));
    }

    public Cancel() {
        this.OnCanceled.emit();
    }
}