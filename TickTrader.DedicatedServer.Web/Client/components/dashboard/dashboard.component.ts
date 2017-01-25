import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable } from "rxjs/Rx";
import { ExtBotModel, BotModel, BotState, ParameterType } from '../../models/index';
import { ApiService } from '../../services/index';
import { Router, ActivatedRoute, Params } from '@angular/router';


@Component({
    selector: 'home',
    template: require('./dashboard.component.html'),
    styles: [require('../app/app.component.css')],
})
export class DashboardComponent implements OnInit {
    botStateType = BotState;
    parameterType = ParameterType;

    allBots: BotModel[];
    dashboardBots: Observable<ExtBotModel[]>;

    selectedBot: BotModel = null;
    selectedExtBot: ExtBotModel = null;

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
        this.router.navigate(['/bot-detail', bot.instanceId]);
    }
}
