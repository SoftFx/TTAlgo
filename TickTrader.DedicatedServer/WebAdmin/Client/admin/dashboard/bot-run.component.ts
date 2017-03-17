import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable } from "rxjs/Rx";
import { PackageModel, PluginModel, ParameterDataTypes, AccountModel, PluginSetupModel } from '../../models/index';
import { ApiService } from '../../services/index';
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


    constructor(private _api: ApiService) { }

    ngOnInit() {
        this._api.GetAccounts().subscribe(response => this.Accounts = response);
        this._api.GetPackages().subscribe(response => this.Packages = response);
    }

    addBot() {
        console.info(this.Setup.Payload);
        this._api.SetupPlugin(this.Setup).subscribe(r => { });
    }

    cancel() {

    }

    onPluginSelected(plugin: PluginModel) {
        this.Setup = PluginSetupModel.Create(plugin);
    }
}