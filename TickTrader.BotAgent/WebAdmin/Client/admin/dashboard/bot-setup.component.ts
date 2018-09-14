import { Component, EventEmitter, Output, Input, OnInit, OnDestroy } from '@angular/core';
import { FormGroup, FormControl, Validators, FormArray, FormBuilder } from '@angular/forms';
import { Location } from '@angular/common';
import { Observable } from "rxjs/Rx";
import { PackageModel, PluginModel, ParameterDataTypes, AccountModel, SetupModel, TradeBotModel, ResponseStatus, ObservableRequest, AccountInfo } from '../../models/index';
import { ApiService, ToastrService } from '../../services/index';

@Component({
    selector: 'bot-setup-cmp',
    template: require('./bot-setup.component.html'),
    styles: [require('../../app.component.css')],
})

export class BotSetupComponent implements OnInit {
    public AccountInfoRequest: ObservableRequest<AccountInfo>;
    public SaveConfigRequest: ObservableRequest<void>;

    public Setup: SetupModel;
    public BotSetupForm: FormGroup;
    public Symbols: string[];

    @Input() TradeBot: TradeBotModel;
    @Output() OnSaved = new EventEmitter<void>();

    constructor(private _fb: FormBuilder, private _api: ApiService, private _toastr: ToastrService, private _location: Location) { }

    ngOnInit() {
        this.SaveConfigRequest = null;
        this.initSetupForm();
        this.AccountInfoRequest = new ObservableRequest(this._api.GetAccountInfo(this.Setup.Account))
            .Subscribe(info => this.Symbols = info.Symbols);
    }

    SaveConfig() {
        if (this.BotSetupForm.valid) {

            this.SaveConfigRequest = new ObservableRequest(this._api.UpdateBotConfig(this.TradeBot.Id, this.Setup))
                .Subscribe(ok => this.OnSaved.emit(),
                err => {
                    if (!err.Handled)
                    {
                        this._toastr.error(err.Message);
                    }
                })
        }
    }

    ResetError() {
        this.SaveConfigRequest = null;
    }

    Cancel() {
        this._location.back();
    }

    private initSetupForm() {
        this.Setup = SetupModel.ForTradeBot(this.TradeBot);
        this.BotSetupForm = this.createGroupForm(this.Setup);
    }

    private createGroupForm(setup: SetupModel) {
        let formGroup = this._fb.group({});

        formGroup.addControl("InstanceId", this._fb.control(setup.InstanceId, Validators.required));
        formGroup.addControl("Isolated", this._fb.control(setup.Permissions.Isolated));
        formGroup.addControl("TradeAllowed", this._fb.control(setup.Permissions.TradeAllowed));
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