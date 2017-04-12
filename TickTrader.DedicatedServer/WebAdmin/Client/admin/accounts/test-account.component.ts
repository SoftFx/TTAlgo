import { Input, EventEmitter, Output, Component, OnInit } from '@angular/core';
import { AccountModel, ConnectionErrorCodes, ConnectionTestResult } from '../../models/index';
import { ApiService } from '../../services/index';

@Component({
    selector: 'test-account-cmp',
    template: require('./test-account.component.html'),
    styles: [require('../../app.component.css')],
})

export class TestAccountComponent implements OnInit {
    public ConnectionErrorCode = ConnectionErrorCodes;
    public TestResult: ConnectionTestResult;
    public ConnectionTestRunning: boolean;

    constructor(private _api: ApiService) { }

    @Input() Account: AccountModel;
    @Output() OnTested = new EventEmitter<ConnectionTestResult>();
    @Output() OnClosed = new EventEmitter<void>();


    ngOnInit() {
        this.resetTestResult();
        this.runTest();
    }

    public Close() {
        this.OnClosed.emit();
    }

    private runTest() {
        let accountClone = Object.assign(new AccountModel(), this.Account);
        this.ConnectionTestRunning = true;

        this._api.TestAccount(accountClone)
            .finally(() => { this.ConnectionTestRunning = false; })
            .subscribe(ok => {
                this.TestResult = new ConnectionTestResult(ok.json());
                this.OnTested.emit(this.TestResult);
            });
    }

    private resetTestResult() {
        this.TestResult = null;
    }
}