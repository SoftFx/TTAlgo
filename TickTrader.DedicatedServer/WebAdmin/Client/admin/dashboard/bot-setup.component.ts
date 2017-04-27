import { Component, EventEmitter, Output, OnInit, OnDestroy } from '@angular/core';
import { FormGroup, FormControl, Validators, FormArray, FormBuilder } from '@angular/forms';
import { Observable } from "rxjs/Rx";
import { PackageModel, PluginModel, ParameterDataTypes, AccountModel, PluginSetupModel, TradeBotModel, ResponseStatus } from '../../models/index';
import { ApiService, ToastrService } from '../../services/index';
import { Router } from '@angular/router';

@Component({
    selector: 'bot-setup-cmp',
    template: require('./bot-setup.component.html'),
    styles: [require('../../app.component.css')],
})

export class BotSetupComponent implements OnInit {
    public Packages: PackageModel[];
    public Accounts: AccountModel[];
    public SelectedPlugin: PluginModel;
    public Setup: PluginSetupModel;
    public BotSetupForm: FormGroup;

    @Output() OnAdded = new EventEmitter<TradeBotModel>();

    constructor(private _fb: FormBuilder, private _api: ApiService, private _toastr: ToastrService) { }

    ngOnInit() {
        this.BotSetupForm = this._fb.group({});

        this._api.GetAccounts().subscribe(response => this.Accounts = response);
        this._api.GetPackages().subscribe(response => this.Packages = response);
    }

    addBot() {
        console.log(JSON.stringify(this.BotSetupForm.value));
        //if (this.Setup) {
        //    this._api.AddBot(this.Setup).subscribe(
        //        tb => this.OnAdded.emit(tb),
        //        err => this.notifyAboutError(err)
        //    );
        //}
    }

    cancel() {
        this.SelectedPlugin = null;
        this.Setup = null;
        this.BotSetupForm.reset();
    }

    onPluginSelected(plugin: PluginModel) {
        this.Setup = PluginSetupModel.Create(plugin);
        this.BotSetupForm = this.createGroupForm(this.Setup);
    }

    private createGroupForm(setup: PluginSetupModel) {
        let formGroup = this._fb.group({});

        formGroup.addControl("Account", this._fb.control(null, Validators.required));
        formGroup.addControl("Symbol", this._fb.control("", Validators.required));
        formGroup.addControl("InstanceId", this._fb.control(setup.InstanceId, Validators.required));

        setup.Parameters.forEach(parameter => formGroup.addControl(parameter.Descriptor.Id,
            this._fb.control(parameter.Descriptor.DefaultValue, parameter.Descriptor.IsRequired? Validators.required : []))
        );
        return formGroup;
    }

    private notifyAboutError(response: ResponseStatus) {
        if (response.Handled)
            this._toastr.error(response.Message);
    }
}