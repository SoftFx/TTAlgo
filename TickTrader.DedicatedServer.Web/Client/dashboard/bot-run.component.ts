import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable } from "rxjs/Rx";
import { ExtBotModel, BotModel, BotState, ParameterType } from '../models/index';
import { ApiService } from '../services/index';
import { Router } from '@angular/router';

@Component({
    selector: 'bot-run-cmp',
    template: require('./bot-run.component.html'),
    styles: [require('../app.component.css')],
})

export class BotRunComponent implements OnInit {
    parameterType = ParameterType;
    allBots: BotModel[];

    selectedBot: BotModel = null;
    bot: ExtBotModel = null;

    constructor(private api: ApiService, private router: Router) { }

    ngOnInit() {
        this.api.loadAllBots().subscribe(data => {
            this.allBots = data;
        });
    }

    addBot(bot: ExtBotModel) {
        this.api.addBot(bot)
            .subscribe(() => {
                this.cancel()
            });
    }

    cancel() {
        this.selectedBot = null;
        this.bot = null;
    }

    onBotSelected(bot: BotModel) {
        this.selectedBot = bot;
        if (bot)
            this.bot = new ExtBotModel(bot.name, bot.setup);
    }
}