import { Input, EventEmitter, Output, Component, OnInit } from '@angular/core';
import { AccountModel, ObservableRequest } from '../../models/index';
import { ApiService, ToastrService } from '../../services/index';
import { FormGroup, FormControl, Validators, FormArray, FormBuilder } from '@angular/forms';

@Component({
    selector: 'add-account-cmp',
    template: require('./add-account.component.html'),
    styles: [require('../../app.component.css')],
})

export class AddAccountComponent implements OnInit {
    public TestAccountEnabled: boolean;
    public AccountForm: FormGroup;
    public AddRequest: ObservableRequest<AccountModel>;

    constructor(private _fBuilder: FormBuilder, private _api: ApiService, private _toastr: ToastrService) { }

    @Output() OnAdded = new EventEmitter<AccountModel>();


    ngOnInit() {
        this.AddRequest = null;

        this.AccountForm = this._fBuilder.group({
            Login: ["", Validators.required],
            Password: ["", Validators.required],
            Server: ["", Validators.required]
        });
    }

    public Add(account: AccountModel) {
        let accountClone = <AccountModel>{ ...account };

        this.AddRequest = new ObservableRequest<AccountModel>(this._api.AddAccount(accountClone))
            .Subscribe(ok => {
                this.Reset();
                this.OnAdded.emit(accountClone);
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
        this.AccountForm.reset();
    }

    public Cancel() {
        this.AddRequest = null;
    }
}
