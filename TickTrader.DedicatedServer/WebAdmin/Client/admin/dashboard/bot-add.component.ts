﻿import { Component, EventEmitter, Output, Input, OnInit, OnDestroy } from '@angular/core';
import { FormGroup, FormControl, Validators, FormArray, FormBuilder } from '@angular/forms';
import { Location } from '@angular/common';
import { Observable, Subject } from "rxjs/Rx";
import { PackageModel, PluginModel, ParameterDataTypes, AccountModel, SetupModel, TradeBotModel, ResponseStatus, ObservableRequest, AccountInfo, ResponseCode } from '../../models/index';
import { ApiService, ToastrService } from '../../services/index';

@Component({
    selector: 'bot-add-cmp',
    template: require('./bot-add.component.html'),
    styles: [require('../../app.component.css')],
})

export class BotAddComponent implements OnInit {
    private _autogenerateBotIdRef: number = 0;
    private _loadSymbolsRef: number = 0;
    private _pluginSelectedSubject = new Subject<PluginModel>();
    private _accountSelectedSubject = new Subject<AccountModel>();

    public AddBotRequest: ObservableRequest<TradeBotModel>;
    public PackagesRequest: ObservableRequest<PackageModel[]>;
    public AccountInfoRequest: ObservableRequest<AccountInfo>;
    public AccountsRequest: ObservableRequest<AccountModel[]>;
    public Symbols: string[];
    public Setup: SetupModel;
    public BotSetupForm: FormGroup;

    @Output() OnAdded = new EventEmitter<TradeBotModel>();

    constructor(private _fb: FormBuilder, private _api: ApiService, private _toastr: ToastrService, private _location: Location) { }

    ngOnInit() {
        this.AddBotRequest = null;

        this._pluginSelectedSubject
            .debounceTime(200)
            .distinctUntilChanged()
            .subscribe(plugin => this.initSetupForm(plugin));

        this._accountSelectedSubject
            .debounceTime(200)
            .distinctUntilChanged()
            .subscribe(account => this.loadSymbols(account));

        this.PackagesRequest = new ObservableRequest(this._api.GetPackages()).Subscribe();
        this.AccountsRequest = new ObservableRequest(this._api.GetAccounts()).Subscribe();
        this.AccountInfoRequest = new ObservableRequest(Observable.of(new AccountInfo())).Subscribe();
    }

    AddBot() {
        if (this.BotSetupForm.valid) {
            this.AddBotRequest = new ObservableRequest(this._api.AddBot(this.Setup))
                .Subscribe(result => this.OnAdded.emit(result),
                err => {
                    if (!err.Handled)
                        this._toastr.error(err.Message);
                    else if (err.Code === ResponseCode.DuplicateBot)
                    {
                        err.Message = `Bot with ID '${this.Setup.InstanceId}' already exists! Please type another ID.`;
                    }
                });
        }
    }

    Cancel() {
        this._location.back();
    }

    ResetError() {
        this.AddBotRequest = null;
    }

    OnPluginSelected(plugin: PluginModel) {
        this._pluginSelectedSubject.next(plugin);
    }

    OnAccountChanged(account: AccountModel) {
        this._accountSelectedSubject.next(account);
    }

    private initSetupForm(plugin: PluginModel) {
        if (plugin) {
            this.Setup = SetupModel.ForPlugin(plugin);
            this.BotSetupForm = this.createSetupForm(this.Setup);

            let localAutogenerateBotIdRef = ++this._autogenerateBotIdRef;

            this._api.AutogenerateBotId(plugin.DisplayName)
                .filter(id => this._autogenerateBotIdRef == localAutogenerateBotIdRef && this.BotSetupForm && !this.BotSetupForm.value.InstanceId)
                .subscribe(id => { if (this.BotSetupForm) this.BotSetupForm.patchValue({ "InstanceId": id }) });
        }
        else {
            this.Setup = null;
            this.BotSetupForm = null;
        }
    }

    private loadSymbols(account: AccountModel) {
        this.Symbols = [];
        this.BotSetupForm.controls.Symbol.reset();

        if (account) {
            let localLoadSymbolsRef = ++this._loadSymbolsRef;

            this.AccountInfoRequest = new ObservableRequest(this._api.GetAccountInfo(account).filter(info => this._loadSymbolsRef == localLoadSymbolsRef))
                .Subscribe(info => {
                    this.Symbols = info.Symbols;
                    this.BotSetupForm.patchValue({ Symbol: info.MainSymbol });
                });
        }
    }

    private createSetupForm(setup: SetupModel) {
        let formGroup = this._fb.group({});

        formGroup.addControl("InstanceId", this._fb.control(setup.InstanceId, Validators.required));
        formGroup.addControl("Isolated", this._fb.control(setup.Isolated));
        formGroup.addControl("Account", this._fb.control(null, Validators.required));
        formGroup.addControl("Symbol", this._fb.control("", Validators.required));

        setup.Parameters.forEach(parameter => formGroup.addControl(parameter.Descriptor.Id,
            this._fb.control(parameter.Value, parameter.Descriptor.IsRequired ? Validators.required : []))
        );
        return formGroup;
    }

    private notifyAboutError(response: ResponseStatus) {
        if (response.Handled)
            this._toastr.error(response.Message);
    }
}