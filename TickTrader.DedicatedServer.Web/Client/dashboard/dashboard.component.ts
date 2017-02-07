import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable } from "rxjs/Rx";
import { ExtBotModel, BotModel, BotState, ParameterType } from '../models/index';
import { ApiService } from '../services/index';
import { Router, ActivatedRoute, Params } from '@angular/router';


@Component({
    selector: 'dashboard-cmp',
    template: require('./dashboard.component.html'),
    styles: [require('../app.component.css')],
})

export class DashboardComponent{
    botStateType = BotState;
    parameterType = ParameterType;

    allBots: BotModel[];
    dashboardBots: Observable<ExtBotModel[]>;

    constructor(private api: ApiService,
        private route: ActivatedRoute,
        private router: Router
    ) { }

    ngOnInit() {
        this.dashboardBots = this.api.dasboardBots;
        this.api.loadBotsOnDashboard();

        this.api.loadAllBots().subscribe(data => {
            this.allBots = data;
        });
    }

    run(bot: ExtBotModel) {
        this.api.runBot(bot);
    }

    stop(bot: ExtBotModel) {
        this.api.stopBot(bot);
    }

    configurate(bot: ExtBotModel) {

    }

    remove(bot: ExtBotModel) {
        this.api.removeBotFromDashboard(bot);
    }

    gotoDetails(bot: ExtBotModel) {
        this.router.navigate(['/bot', bot.instanceId]);
    }
}
