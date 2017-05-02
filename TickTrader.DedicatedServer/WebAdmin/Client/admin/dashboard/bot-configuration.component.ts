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
        this._route.params
            .filter((params: Params) => params['id'])
            .switchMap((params: Params) => this._api.GetTradeBot(params['id']))
            .subscribe((bot: TradeBotModel) => this.Bot = bot);
    }

    public OnSaved(bot: TradeBotModel) {
        this._location.back();
    }

    public OnAdded(bot: TradeBotModel) {
        this._location.back();
    }
}