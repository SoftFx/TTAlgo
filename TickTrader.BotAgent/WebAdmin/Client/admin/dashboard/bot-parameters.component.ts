import { Input, Component, OnInit } from '@angular/core';
import { PluginModel, ParameterDataTypes, AccountModel, SetupModel } from '../../models/index';
import { FormGroup } from '@angular/forms';
import { ApiService } from '../../services/index';

@Component({
    selector: 'bot-parameters-cmp',
    template: require('./bot-parameters.component.html'),
    styles: [require('../../app.component.css')],
})

export class BotParametersComponent {
    public ParameterDataType = ParameterDataTypes;

    @Input() Setup: SetupModel;
    @Input() BotSetupForm: FormGroup;
}