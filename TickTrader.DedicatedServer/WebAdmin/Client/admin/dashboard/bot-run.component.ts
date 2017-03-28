import { Component, EventEmitter, Output, OnInit, OnDestroy } from '@angular/core';
import { Observable } from "rxjs/Rx";
import { PackageModel, PluginModel, ParameterDataTypes, AccountModel, PluginSetupModel, TradeBotModel } from '../../models/index';
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
        this._api.GetAccounts().subscribe(response => this.Accounts = response);
        this._api.GetPackages().subscribe(response => this.Packages = response);
    }

    addBot() {
        console.info(this.Setup.Payload);
        this._api.SetupPlugin(this.Setup).subscribe(
            tb => this.OnAdded.emit(tb),
            err => {
                if (!err.Handled)
                    this._toastr.error(err.Message);
            });
    }

    cancel() {

    }

    onPluginSelected(plugin: PluginModel) {
        this.Setup = PluginSetupModel.Create(plugin);
    }
}