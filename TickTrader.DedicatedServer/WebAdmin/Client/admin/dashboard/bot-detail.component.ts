import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable } from "rxjs/Rx";
import { ApiService } from '../../services/index';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { TradeBotModel, TradeBotLog, ObservableRequest, TradeBotStates } from '../../models/index';

@Component({
    selector: 'bot-detail-cmp',
    template: require('./bot-detail.component.html'),
    styles: [require('../../app.component.css')]
})

export class BotDetailComponent implements OnInit {
    public TradeBotState = TradeBotStates;
    public Bot: TradeBotModel;
    public Log: TradeBotLog;

    public BotRequest: ObservableRequest<TradeBotModel>;
    public LogRequest: ObservableRequest<TradeBotLog>;

    constructor(
        private _route: ActivatedRoute,
        private _api: ApiService
    ) { }

    ngOnInit() {
        this._route.params
            .subscribe((params: Params) => {
                this.BotRequest = new ObservableRequest(params['id'] ?
                    this._api.GetTradeBot(params['id']) :
                    Observable.of(<TradeBotModel>null)
                ).Subscribe(result => this.Bot = result);

                this.LogRequest = new ObservableRequest(params['id'] ?
                    this._api.GetTradeBotLog(params['id']) :
                    Observable.of(<TradeBotLog>null)
                ).Subscribe(result => this.Log = result);
            });
    }
}