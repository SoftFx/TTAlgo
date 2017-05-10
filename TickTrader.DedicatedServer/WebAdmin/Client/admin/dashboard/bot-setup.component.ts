import { Component, EventEmitter, Output, Input, OnInit, OnDestroy } from '@angular/core';
import { FormGroup, FormControl, Validators, FormArray, FormBuilder } from '@angular/forms';

import { Observable } from "rxjs/Rx";
import { PackageModel, PluginModel, ParameterDataTypes, AccountModel, SetupModel, TradeBotModel, ResponseStatus } from '../../models/index';
import { ApiService, ToastrService } from '../../services/index';
import { Router } from '@angular/router';

@Component({
    selector: 'bot-setup-cmp',
    template: require('./bot-setup.component.html'),
    styles: [require('../../app.component.css')],
})

export class BotSetupComponent implements OnInit {
    public Setup: SetupModel;
    public BotSetupForm: FormGroup;
    public Symbols: string[];

    @Input() TradeBot: TradeBotModel;
    @Output() OnSaved = new EventEmitter<TradeBotModel>();

    constructor(private _fb: FormBuilder, private _api: ApiService, private _toastr: ToastrService) { }

    ngOnInit() {
        this.BotSetupForm = this._fb.group({});

        this.Setup = SetupModel.ForTradeBot(this.TradeBot);
        this.BotSetupForm = this.createGroupForm(this.Setup);

        this._api.GetSymbols(this.Setup.Account).subscribe(symbols => this.Symbols = symbols);
    }

    applyConfig() {
        if (this.BotSetupForm.valid) {
            this._api.UpdateBotConfig(this.TradeBot.Id, this.Setup).subscribe(
                tb => this.OnSaved.emit(tb),
                err => this.notifyAboutError(err)
            );
        }
    }

    cancel() {
        this.Setup = null;
        this.BotSetupForm.reset();
    }

    private createGroupForm(setup: SetupModel) {
        let formGroup = this._fb.group({});

        formGroup.addControl("InstanceId", this._fb.control(setup.InstanceId, Validators.required));
        formGroup.addControl("Account", this._fb.control(setup.Account, Validators.required));
        formGroup.addControl("Symbol", this._fb.control(setup.Symbol, Validators.required));

        setup.Parameters.forEach(parameter => formGroup.addControl(parameter.Descriptor.Id,
            this._fb.control(this.getParamValue(parameter.Descriptor.Id), parameter.Descriptor.IsRequired ? Validators.required : []))
        );
        return formGroup;
    }

    private getParamValue(paramId: string): Object
    {
        let parameter = this.Setup.Parameters.find(p => p.Descriptor.Id === paramId);
        return parameter.Value;
    }

    private notifyAboutError(response: ResponseStatus) {
        if (response.Handled)
            this._toastr.error(response.Message);
    }
}