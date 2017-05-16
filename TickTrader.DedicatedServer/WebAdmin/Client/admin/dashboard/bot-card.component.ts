﻿import { Component, EventEmitter, Input, Output, OnInit, OnDestroy } from '@angular/core';
import { Observable } from "rxjs/Rx";
import { TradeBotModel, TradeBotStates, TradeBotStateModel, ResponseStatus } from '../../models/index';
import { ApiService, ToastrService, FeedService } from '../../services/index';
import { Router } from '@angular/router';

@Component({
    selector: 'bot-card-cmp',
    template: require('./bot-card.component.html'),
    styles: [require('../../app.component.css')],
})

export class BotCardComponent implements OnInit {
    public TradeBotState = TradeBotStates;

    @Input() TradeBot: TradeBotModel;
    @Output() OnDeleted = new EventEmitter<TradeBotModel>();

    constructor(private _api: ApiService, private _toastr: ToastrService, private _router: Router) { }

    ngOnInit() {
        this._api.Feed.ChangeBotState.subscribe(botState => this.updateBotState(botState));
    }

    public get CanStop(): boolean {
        return this.TradeBot.State === TradeBotStates.Online
            || this.TradeBot.State === TradeBotStates.Starting
            || this.TradeBot.State === TradeBotStates.Started;
    }

    public get CanStart(): boolean {
        return this.TradeBot.State === TradeBotStates.Offline
            || this.TradeBot.State === TradeBotStates.Faulted;
    }

    public get CanDelete(): boolean {
        return this.TradeBot.State === TradeBotStates.Offline
            || this.TradeBot.State === TradeBotStates.Faulted;
    }

    public get CanConfigurate(): boolean {
        return this.TradeBot.State === TradeBotStates.Offline
            || this.TradeBot.State === TradeBotStates.Faulted;
    }

    public Start() {
        this.TradeBot = <TradeBotModel>{ ...this.TradeBot, 'State': TradeBotStates.Starting }

        this._api.StartBot(this.TradeBot.Id).subscribe(
            ok => { },
            err => this.notifyAboutError(err)
        );
    }

    public Stop() {
        this._api.StopBot(this.TradeBot.Id).subscribe(
            ok => { },
            err => this.notifyAboutError(err)
        );
    }

    public Delete() {
        this._api.DeleteBot(this.TradeBot.Id).subscribe(
            ok => this.OnDeleted.emit(this.TradeBot),
            err => this.notifyAboutError(err)
        );
    }

    public Configurate(instanceId: string) {
        if (instanceId)
            this._router.navigate(['/configurate', instanceId]);
    }

    private updateBotState(botState: TradeBotStateModel) {
        if (this.TradeBot.Id === botState.Id) {
            this.TradeBot = <TradeBotModel>{ ...this.TradeBot, 'State': botState.State }
        }
    }

    private notifyAboutError(response: ResponseStatus) {
        this._toastr.error(response.Message);
    }
}
