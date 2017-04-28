import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable } from "rxjs/Rx";
import { TradeBotModel } from '../../models/index';
import { ApiService } from '../../services/index';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { Location } from '@angular/common';

@Component({
    selector: 'bot-configuration-cmp',
    template: require('./bot-configuration.component.html'),
    styles: [require('../../app.component.css')],
})

export class BotConfigurationComponent implements OnInit {
    public Bot: TradeBotModel;

    constructor(
        private _route: ActivatedRoute,
        private _router: Router,
        private _api: ApiService,
        private _location: Location
    ) { }

    ngOnInit() {
        if (this._route.params['id'])
            this._route.params
                .switchMap((params: Params) => this._api.GetTradeBot(params['id']))
                .subscribe((bot: TradeBotModel) => this.Bot = bot);
    }

    public OnTradeBotAdded(bot: TradeBotModel) {
        this._location.back();
    }
}