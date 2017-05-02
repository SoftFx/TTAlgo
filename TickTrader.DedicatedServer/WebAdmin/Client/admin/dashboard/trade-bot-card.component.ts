import { Component, EventEmitter, Input, Output, OnInit, OnDestroy } from '@angular/core';
import { Observable } from "rxjs/Rx";
import { TradeBotModel, TradeBotStates, TradeBotStateModel, ResponseStatus } from '../../models/index';
import { ApiService, ToastrService, FeedService } from '../../services/index';

@Component({
    selector: 'trade-bot-card-cmp',
    template: require('./trade-bot-card.component.html'),
    styles: [require('../../app.component.css')],
})

export class TradeBotCardComponent implements OnInit {
    public TradeBotState = TradeBotStates;
    @Input() TradeBot: TradeBotModel;
    @Output() OnDeleted = new EventEmitter<TradeBotModel>();

    constructor(private _api: ApiService, private _toastr: ToastrService) { }

    ngOnInit() {
        this._api.Feed.changeBotState.subscribe(botState => this.updateBotState(botState));
    }

    public Start() {
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

    private updateBotState(botState: TradeBotStateModel) {
        if (this.TradeBot.Id === botState.Id) {
            this.TradeBot = <TradeBotModel>{ ...this.TradeBot, 'State': botState.State }
        }
    }

    private notifyAboutError(response: ResponseStatus) {
        this._toastr.error(response.Message);
    }
}
