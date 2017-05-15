import { OnInit, Component } from '@angular/core';
import { FormGroup, FormControl, Validators, FormArray, FormBuilder } from '@angular/forms';
import { ApiService, ToastrService } from '../../services/index';
import { AccountModel, ResponseStatus, ResponseCode, ConnectionErrorCodes, ConnectionTestResult } from '../../models/index';

@Component({
    selector: 'accounts-cmp',
    template: require('./accounts.component.html'),
    styles: [require('../../app.component.css')]
})

export class AccountsComponent implements OnInit {
    public Accounts: AccountModel[];
    public TestAccountEnabled: boolean;
    public AccountForm: FormGroup;

    constructor(private _fBuilder: FormBuilder, private _api: ApiService, private _toastr: ToastrService) {
    }

    ngOnInit() {
        this.AccountForm = this._fBuilder.group({
            Login: ["", Validators.required],
            Password: ["", Validators.required],
            Server: ["", Validators.required]
        });

        this.Accounts = [];

        this._api.Feed.AddAccount.subscribe(acc => this.addAccount(acc));
        this._api.Feed.DeleteAccount.subscribe(acc => this.deleteAccount(acc));

        this._api.GetAccounts()
            .subscribe(result => this.Accounts = result);
    }

    public Add(account: AccountModel, isValid: boolean) {
        if (isValid) {
            let accountClone = Object.assign(new AccountModel(), account);
            this._api.AddAccount(accountClone)
                .subscribe(ok => {
                    this.addAccount(accountClone);
                    this.resetForm();
                },
                err => {
                    if (err.Handled)
                        this._toastr.error(err.Message);
                });
        }
    }

    public Delete(account: AccountModel) {
        this.deleteAccount(account);
    }

    public Cancel() {
        this.resetForm();
    }

    public TestAccount(account: AccountModel) {
        this.TestAccountEnabled = true;
    }

    public TestAccountClosed() {
        this.TestAccountEnabled = false;
    }

    private addAccount(account: AccountModel) {
        if (!this.Accounts.find(a => a.Login === account.Login && a.Server === account.Server)) {
            this.Accounts = this.Accounts.concat(account);
        }
    }

    private deleteAccount(account: AccountModel) {
        this.Accounts = this.Accounts.filter(a => !(a.Login === account.Login && a.Server === account.Server));
    }

    private resetForm() {
        this.AccountForm.reset();
    }
}