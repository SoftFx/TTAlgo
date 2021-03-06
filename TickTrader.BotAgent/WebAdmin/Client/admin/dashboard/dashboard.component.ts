import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable } from "rxjs/Rx";
import { TradeBotModel, TradeBotStates, ConnectionStatus } from '../../models/index';
import { ApiService } from '../../services/index';
import { Router, ActivatedRoute, Params } from '@angular/router';


@Component({
    selector: 'dashboard-cmp',
    template: require('./dashboard.component.html'),
    styles: [require('../../app.component.css')],
})

export class DashboardComponent implements OnInit {
    public BotStatesType = TradeBotStates;
    public TradeBots: TradeBotModel[];

    constructor(private _api: ApiService, private _route: ActivatedRoute, private _router: Router) { }

    ngOnInit() {
        this._api.Feed.AddBot.subscribe(bot => this.addBot(bot));
        this._api.Feed.DeleteBot.subscribe(id => this.deleteBot(id));
        this._api.Feed.ConnectionState.subscribe(state => { if (state == ConnectionStatus.Connected) this.loadBots(); });

        this.loadBots();
    }

    OnTradeBotAdded(bot: TradeBotModel) {
        this.addBot(bot);
    }

    OnTradeBotDeleted(bot: TradeBotModel) {
        this.deleteBot(bot.Id);
    }

    public Configurate() {
        this._router.navigate(['/configurate']);
    }

    private loadBots() {
        this._api.GetTradeBots().subscribe(res => this.TradeBots = res);
    }

    private addBot(bot: TradeBotModel) {
        if (!this.TradeBots)
            this.TradeBots = [];

        if (!this.TradeBots.find(b => b.Id === bot.Id)) {
            this.TradeBots = this.TradeBots.concat(bot);
        }
    }

    private deleteBot(id: string) {
        if (this.TradeBots)
            this.TradeBots = this.TradeBots.filter(x => x.Id !== id);
    }
}
