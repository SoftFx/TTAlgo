import { Input, EventEmitter, Output, Component, OnInit } from '@angular/core';
import { AccountModel, ConnectionErrorCodes, ConnectionTestResult, ObservableRequest } from '../../models/index';
import { ApiService, ToastrService } from '../../services/index';
import { FormGroup, FormControl, Validators, FormArray, FormBuilder } from '@angular/forms';

@Component({
    selector: 'account-change-password-cmp',
    template: require('./account-change-password.component.html'),
    styles: [require('../../app.component.css')],
})

export class AccountChangePasswordComponent implements OnInit {
    public ChangePasswordForm: FormGroup;

    public ChangePasswordRequest: ObservableRequest<void>;

    constructor(private _fBuilder: FormBuilder, private _api: ApiService, private _toastr: ToastrService) { }

    @Input() Account: AccountModel;
    @Output() OnChanged = new EventEmitter<void>();
    @Output() OnCanceled = new EventEmitter<void>();

    ngOnInit() {
        this.ChangePasswordRequest = null;

        this.ChangePasswordForm = this._fBuilder.group({
            "Password": ["", Validators.required]
        });
    }

    public ChangePassword(password: string) {
        let account = <AccountModel>{ ... this.Account, Password: password };

        this.ChangePasswordRequest = new ObservableRequest(this._api.ChangeAccountPassword(account))
            .Subscribe(ok => this.OnChanged.emit(),
            err => {
                if (!err.Handled) {
                    this._toastr.error(err.Message);
                    this.ChangePasswordRequest = null;
                }
            })
    }

    public Cancel() {
        this.OnCanceled.emit();
    }
}