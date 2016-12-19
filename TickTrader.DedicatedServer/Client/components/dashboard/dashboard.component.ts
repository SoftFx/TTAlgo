import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable } from "rxjs/Rx";
import { ExtBotModel, BotModel, BotState } from '../../models/index';
import { ApiService } from '../../services/index';


@Component({
    selector: 'home',
    template: require('./dashboard.component.html'),
    styles: [require('../app/app.component.css')],
})
export class DashboardComponent implements OnInit {
    allBots: BotModel[];
    dashboardBots: Observable<ExtBotModel[]>;
    botStateType = BotState;
    BotModel: BotModel;
    selectedBot: BotModel = new BotModel(-1, '');

    constructor(private api: ApiService) {
    }

    ngOnInit() {
        this.dashboardBots = this.api.dasboardBots;
        this.api.loadBotsOnDashboard();

        this.api.loadAllBots().subscribe(data => {
            this.allBots = data;
        });
    }

    runBot(bot: ExtBotModel) {
        this.api.runBot(bot);
    }

    stopBot(bot: ExtBotModel) {
        this.api.stopBot(bot);
    }

    removeBot(bot: ExtBotModel) {
        this.api.removeBotFromDashboard(bot);
    }
}
