import { Component, EventEmitter, Output, Input, OnInit, OnDestroy } from '@angular/core';
import { FormGroup, FormControl, Validators, FormArray, FormBuilder } from '@angular/forms';

import { Observable } from "rxjs/Rx";
import { PackageModel, PluginModel, ParameterDataTypes, AccountModel, SetupModel, TradeBotModel, ResponseStatus, ObservableRequest } from '../../models/index';
import { ApiService, ToastrService } from '../../services/index';
import { Router } from '@angular/router';

@Component({
    selector: 'bot-add-cmp',
    template: require('./bot-add.component.html'),
    styles: [require('../../app.component.css')],
})

export class BotAddComponent implements OnInit {
    private _autogenerateBotIdRef: number = 0;

    public PackagesRequest: ObservableRequest<PackageModel[]>;
    public AccountsRequest: ObservableRequest<AccountModel[]>;
    public Symbols: string[];
    public SelectedPlugin: PluginModel;
    public Setup: SetupModel;
    public BotSetupForm: FormGroup;

    @Output() OnAdded = new EventEmitter<TradeBotModel>();

    constructor(private _fb: FormBuilder, private _api: ApiService, private _toastr: ToastrService) { }

    ngOnInit() {
        this.BotSetupForm = this._fb.group({});

        this.PackagesRequest = new ObservableRequest(this._api.GetPackages()).Subscribe();
        this.AccountsRequest = new ObservableRequest(this._api.GetAccounts()).Subscribe();
    }

    AddBot() {
        if (this.BotSetupForm.valid) {
            this.applSetupForm();

            this._api.AddBot(this.Setup).subscribe(
                tb => this.OnAdded.emit(tb),
                err => this.notifyAboutError(err)
            );
        }
    }

    Cancel() {
        this.SelectedPlugin = null;
        this.Setup = null;
        this.BotSetupForm.reset();
    }

    OnPluginSelected(plugin: PluginModel) {
        this.Setup = SetupModel.ForPlugin(plugin);
        this.BotSetupForm = this.initSetupForm(this.Setup);

        let _localAutogenerateBotIdRef = ++this._autogenerateBotIdRef;

        this._api.AutogenerateBotId(plugin.DisplayName)
            .filter(id => this._autogenerateBotIdRef == _localAutogenerateBotIdRef && !this.BotSetupForm.value.InstanceId)
            .subscribe(id => this.BotSetupForm.patchValue({ "InstanceId": id }));
    }

    OnAccountChanged(account: AccountModel) {
        this._api.GetSymbols(account).subscribe(symbols => this.Symbols = symbols);
    }

    private applSetupForm() {
        this.Setup.Account = this.BotSetupForm.value.Account;
        this.Setup.Symbol = this.BotSetupForm.value.Symbol;
        this.Setup.InstanceId = this.BotSetupForm.value.InstanceId;
        this.Setup.Parameters.forEach(p => {
            p.Value = this.BotSetupForm.value[p.Descriptor.Id];
        })
    }

    private initSetupForm(setup: SetupModel) {
        let formGroup = this._fb.group({});

        formGroup.addControl("InstanceId", this._fb.control(setup.InstanceId, Validators.required));
        formGroup.addControl("Account", this._fb.control(null, Validators.required));
        formGroup.addControl("Symbol", this._fb.control("", Validators.required));

        setup.Parameters.forEach(parameter => formGroup.addControl(parameter.Descriptor.Id,
            this._fb.control(parameter.Descriptor.DefaultValue, parameter.Descriptor.IsRequired ? Validators.required : []))
        );
        return formGroup;
    }

    private notifyAboutError(response: ResponseStatus) {
        if (response.Handled)
            this._toastr.error(response.Message);
    }
}