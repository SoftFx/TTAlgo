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
        this._api.Feed.addBot.subscribe(bot => this.addBot(bot));
        this._api.Feed.deleteBot.subscribe(id => this.deleteBot(id));

        this._api.GetTradeBots().subscribe(res => this.TradeBots = res);
    }

    OnTradeBotAdded(bot: TradeBotModel) {
        this.addBot(bot);
    }

    OnTradeBotDeleted(bot: TradeBotModel) {
        this.deleteBot(bot.Id);
    }

    //gotoDetails(bot: ExtBotModel) {
    //    this.router.navigate(['/bot', bot.instanceId]);
    //}

    public Configurate() {
        this._router.navigate(['/configurate']);
    }

    private addBot(bot: TradeBotModel) {
        if (!this.TradeBots.find(b => b.Id === bot.Id)) {
            this.TradeBots = this.TradeBots.concat(bot);
        }
    }

    private deleteBot(id: string) {
        this.TradeBots = this.TradeBots.filter(x => x.Id !== id);
    }

}
