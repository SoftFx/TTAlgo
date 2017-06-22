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
    public ErrorMessage: string;
    public IsEditMode: boolean;
    public IsCreateMode: boolean;

    constructor(
        private _route: ActivatedRoute,
        private _router: Router,
        private _api: ApiService,
        private _location: Location
    ) { }

    ngOnInit() {
        this._route.params
            .switchMap((params: Params) => params['id'] ? this._api.GetTradeBot(params['id']) : Observable.of(null))
            .subscribe((bot: TradeBotModel) => {
                this.Bot = bot;
                this.updateModeOfComponent();
            },
            err => { this.ErrorMessage = err.Message; });
    }

    public OnSaved() {
        this._location.back();
    }

    public OnAdded(bot: TradeBotModel) {
        this._location.back();
    }

    private updateModeOfComponent() {
        if (this.Bot) {
            this.IsCreateMode = false;
            this.IsEditMode = true;
        }
        else {
            this.IsCreateMode = true;
            this.IsEditMode = false;
        }
    }
}