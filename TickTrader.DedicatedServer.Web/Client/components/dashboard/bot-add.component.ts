import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable } from "rxjs/Rx";
import { ExtBotModel, BotModel, BotState, ParameterType } from '../../models/index';
import { ApiService } from '../../services/index';
import { Router } from '@angular/router';

@Component({
    selector: 'bot-add',
    template: require('./bot-add.component.html'),
    styles: [require('../app/app.component.css')],
})

export class BotAddComponent implements OnInit {
    parameterType = ParameterType;
    allBots: BotModel[];

    selectedBot: BotModel = null;
    selectedExtBot: ExtBotModel = null;

    constructor(private api: ApiService,
        private router: Router
    ) { }

    ngOnInit() {
        this.api.loadAllBots().subscribe(data => {
            this.allBots = data;
        });
    }

    addBot(bot: ExtBotModel) {
        this.api.addBot(bot)
            .subscribe(() => {
                this.cancel()
                this.gotoDashboard();
            });
    }

    gotoDashboard() {
        this.router.navigate(['/dashboard'])
    }

    cancel() {
        this.selectedBot = null;
        this.selectedExtBot = null;
    }

    onBotSelected(bot: BotModel) {
        this.selectedBot = bot;
        if (bot)
            this.selectedExtBot = new ExtBotModel(bot.name, bot.setup);
    }
}