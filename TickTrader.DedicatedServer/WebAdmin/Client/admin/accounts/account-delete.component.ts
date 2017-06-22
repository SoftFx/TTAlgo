import { Input, EventEmitter, Output, Component, OnInit } from '@angular/core';
import { AccountModel, ObservableRequest} from '../../models/index';
import { ApiService, ToastrService } from '../../services/index';

@Component({
    selector: 'account-delete-cmp',
    template: require('./account-delete.component.html'),
    styles: [require('../../app.component.css')],
})

export class AccountDeleteComponent implements OnInit {

    public DeleteRequest: ObservableRequest<void>;

    constructor(private _api: ApiService, private _toastr: ToastrService) { }

    @Input() Account: AccountModel;
    @Output() OnDeleted = new EventEmitter<AccountModel>();
    @Output() OnCanceled = new EventEmitter<void>();


    ngOnInit() {
        this.DeleteRequest = null;
    }

    public Delete() {
        this.DeleteRequest = new ObservableRequest(this._api.DeleteAccount(<AccountModel>{ ...this.Account }))
            .Subscribe(ok => this.OnDeleted.emit(this.Account),
            err => {
                if (!err.Handled) {
                    this._toastr.error(err.Message);
                    this.DeleteRequest = null;
                }
            })
    }
    
    public Cancel() {
        this.OnCanceled.emit();
    }
}
