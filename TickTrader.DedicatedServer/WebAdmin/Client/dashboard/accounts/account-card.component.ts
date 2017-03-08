import { Input, EventEmitter, Output, Component } from '@angular/core';
import { AccountModel } from '../../models/index';
import { ApiService, ToastrService } from '../../services/index';

@Component({
    selector: 'account-card-cmp',
    template: require('./account-card.component.html'),
    styles: [require('../../app.component.css')],
})

export class AccountCardComponent {

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
        this._api.DeleteAccount(<AccountModel>Object.assign({}, this.Account))
            .subscribe(ok => this.onDeleted.emit(this.Account),
            err => this._toastr.error(`Error deleting account ${this.Account.Login} (${this.Account.Server})`));
    }
}