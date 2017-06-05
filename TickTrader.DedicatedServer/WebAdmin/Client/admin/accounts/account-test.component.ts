import { Input, EventEmitter, Output, Component, OnInit } from '@angular/core';
import { AccountModel, ConnectionErrorCodes, ConnectionTestResult, ObservableRequest } from '../../models/index';
import { ApiService, ToastrService } from '../../services/index';

@Component({
    selector: 'account-test-cmp',
    template: require('./account-test.component.html'),
    styles: [require('../../app.component.css')],
})

export class AccountTestComponent implements OnInit {
    public ConnectionErrorCode = ConnectionErrorCodes;
    public TestResult: ConnectionTestResult;

    public TestRequest: ObservableRequest<ConnectionTestResult>;

    constructor(private _api: ApiService, private _toastr: ToastrService) { }

    @Input() Account: AccountModel;
    @Output() OnTested = new EventEmitter<ConnectionTestResult>();
    @Output() OnCanceled = new EventEmitter<void>();


    ngOnInit() {
        this.TestRequest = new ObservableRequest(this._api.TestAccount(<AccountModel>{ ...this.Account }))
            .Subscribe(ok => this.OnTested.emit(this.TestResult),
            err => {
                if (!err.Handled) {
                    this._toastr.error(err.Message);
                    this.Cancel();
                }
            });
    }

    public Cancel() {
        this.OnCanceled.emit();
    }
}