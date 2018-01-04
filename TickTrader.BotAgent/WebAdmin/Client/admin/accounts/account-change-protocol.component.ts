import { Input, EventEmitter, Output, Component, OnInit } from '@angular/core';
import { AccountModel, ConnectionErrorCodes, ConnectionTestResult, ObservableRequest } from '../../models/index';
import { ApiService, ToastrService } from '../../services/index';
import { FormGroup, FormControl, Validators, FormArray, FormBuilder } from '@angular/forms';

@Component({
    selector: 'account-change-protocol-cmp',
    template: require('./account-change-protocol.component.html'),
    styles: [require('../../app.component.css')],
})

export class AccountChangeProtocolComponent implements OnInit {

    public ChangeProtocolRequest: ObservableRequest<void>;
    public NewProtocol: string;

    constructor(private _fBuilder: FormBuilder, private _api: ApiService, private _toastr: ToastrService) { }

    @Input() Account: AccountModel;
    @Output() OnChanged = new EventEmitter<void>();
    @Output() OnCanceled = new EventEmitter<void>();

    ngOnInit() {
        this.ChangeProtocolRequest = null;

        this.NewProtocol = this.Account.UseNewProtocol ? "FIX" : "SFX";
    }

    public ChangeProtocol() {
        this.ChangeProtocolRequest = new ObservableRequest(this._api.ChangeAccountProtocol(<AccountModel>{ ...this.Account }))
            .Subscribe(ok => {
                this.OnChanged.emit();
                this.Account.UseNewProtocol = !this.Account.UseNewProtocol;
            },
            err => {
                if (!err.Handled) {
                    this._toastr.error(err.Message);
                    this.ChangeProtocolRequest = null;
                }
            })
    }

    public Cancel() {
        this.OnCanceled.emit();
    }
}