import { Component, EventEmitter, Output, OnInit, OnDestroy } from '@angular/core';
import { Observable } from "rxjs/Rx";
import { PackageModel, PluginModel, ParameterDataTypes, AccountModel, PluginSetupModel, TradeBotModel, ResponseStatus } from '../../models/index';
import { ApiService, ToastrService } from '../../services/index';
import { Router } from '@angular/router';

@Component({
    selector: 'bot-run-cmp',
    template: require('./bot-run.component.html'),
    styles: [require('../../app.component.css')],
})

export class BotRunComponent implements OnInit {
    public Packages: PackageModel[];
    public Accounts: AccountModel[];
    public SelectedPlugin: PluginModel;
    public Setup: PluginSetupModel;

    @Output() OnAdded = new EventEmitter<TradeBotModel>();

    constructor(private _api: ApiService, private _toastr: ToastrService) { }

    ngOnInit() {
        console.log(this.SelectedPlugin);
        console.log(this.Setup);
        this._api.GetAccounts().subscribe(response => this.Accounts = response);
        this._api.GetPackages().subscribe(response => this.Packages = response);
    }

    addBot() {
        if (this.Setup) {
            this._api.AddBot(this.Setup).subscribe(
                tb => this.OnAdded.emit(tb),
                err => this.notifyAboutError(err)
            );
        }
    }

    cancel() {
        this.SelectedPlugin = null;
        this.Setup = null;
    }

    onPluginSelected(plugin: PluginModel) {
        this.Setup = PluginSetupModel.Create(plugin);
    }

    private notifyAboutError(response: ResponseStatus) {
        if (response.Handled)
            this._toastr.error(response.Message);
    }
}