import { Input, EventEmitter, Output, Component, OnInit } from '@angular/core';
import { AccountModel, ConnectionErrorCodes, ConnectionTestResult } from '../../models/index';
import { ApiService, ToastrService } from '../../services/index';
import { FormGroup, FormControl, Validators, FormArray, FormBuilder } from '@angular/forms';

@Component({
    selector: 'change-password-cmp',
    template: require('./change-password.component.html'),
    styles: [require('../../app.component.css')],
})

export class ChangePasswordComponent implements OnInit {
    public ChangePasswordForm: FormGroup;

    constructor(private _fBuilder: FormBuilder, private _api: ApiService, private _toastr: ToastrService) { }

    @Input() Account: AccountModel;
    @Output() OnChanged = new EventEmitter<void>();
    @Output() OnCanceled = new EventEmitter<void>();

    ngOnInit() {
        this.ChangePasswordForm = this._fBuilder.group({
            "Password": ["", Validators.required]
        });
    }

    public ChangePassword(password: string) {
        let account = new AccountModel();
        account.Login = this.Account.Login;
        account.Server = this.Account.Server;
        account.Password = password;

        this._api.ChangeAccountPassword(account)
            .subscribe(ok => this.OnChanged.emit(),
            err => this._toastr.error(`Error changing account password ${account.Login} (${account.Server})`));
    }

    public Cancel() {
        this.OnCanceled.emit();
    }
}