import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable } from "rxjs/Rx";
import { TradeBotModel, TradeBotStates } from '../../models/index';
import { ApiService } from '../../services/index';
import { Router, ActivatedRoute, Params } from '@angular/router';


@Component({
    selector: 'dashboard-cmp',
    template: require('./dashboard.component.html'),
    styles: [require('../../app.component.css')],
})

export class DashboardComponent {
    BotStatesType = TradeBotStates;
    TradeBots: TradeBotModel[];

    constructor(private _api: ApiService, private _route: ActivatedRoute, private _router: Router) { }

    ngOnInit() {
        this._api.GetTradeBots().subscribe(res => this.TradeBots = res);
    }

    OnTradeBotAdded(bot: TradeBotModel) {
        this.TradeBots.push(bot);
    }

    OnTradeBotDeleted(bot: TradeBotModel) {
        this.TradeBots = this.TradeBots.filter(x => x.Id !== bot.Id);
    }

    //gotoDetails(bot: ExtBotModel) {
    //    this.router.navigate(['/bot', bot.instanceId]);
    //}
}
