import { Input, Component } from '@angular/core';
import { ExtBotModel, BotModel, BotState, ParameterType } from '../models/index';

@Component({
    selector: 'bot-settings-cmp',
    template: require('./bot-settings.component.html'),
    styles: [require('../app.component.css')],
})

export class BotSettingsComponent {
    parameterType = ParameterType;
    botStateType = BotState;

    @Input() bot: ExtBotModel;
}