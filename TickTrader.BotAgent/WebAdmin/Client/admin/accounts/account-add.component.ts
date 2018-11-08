import { Input, EventEmitter, Output, Component, OnInit } from '@angular/core';
import { AccountModel, ObservableRequest } from '../../models/index';
import { ApiService, ToastrService } from '../../services/index';
import { FormGroup, FormControl, Validators, FormArray, FormBuilder } from '@angular/forms';
import { Location } from '@angular/common';

@Component({
    selector: 'account-add-cmp',
    template: require('./account-add.component.html'),
    styles: [require('../../app.component.css')],
})

export class AccountAddComponent implements OnInit {
    public TestAccountEnabled: boolean;
    public AccountForm: FormGroup;
    public AddRequest: ObservableRequest<AccountModel>;

    constructor(private _fBuilder: FormBuilder, private _api: ApiService, private _toastr: ToastrService, private _location: Location) { }

    @Output() OnAdded = new EventEmitter<AccountModel>();


    ngOnInit() {
        this.AddRequest = null;

        this.AccountForm = this._fBuilder.group({
            Login: ["", Validators.required],
            Password: ["", Validators.required],
            Server: ["", Validators.required],
            UseNewProtocol: false
        });
    }

    public Add(account: AccountModel) {
        let accountClone = <AccountModel>{ ...account };

        this.AddRequest = new ObservableRequest<AccountModel>(this._api.AddAccount(accountClone))
            .Subscribe(ok => {
                this.Reset();
                accountClone.Password = ""; // reset password for local clone
                this.OnAdded.emit(accountClone);
                this._location.back();
            },
            err => {
                if (!err.Handled) {
                    this._toastr.error(err.Message);
                    this.Cancel();
                }
            });
    }

    public TestAccount(account: AccountModel) {
        this.TestAccountEnabled = true;
    }

    public TestCanceled() {
        this.TestAccountEnabled = false;
    }

    public Reset() {
        this.AccountForm.reset({ UseNewProtocol: false });
    }

    public Cancel() {
        this.AddRequest = null;
        this._location.back();
    }
}
