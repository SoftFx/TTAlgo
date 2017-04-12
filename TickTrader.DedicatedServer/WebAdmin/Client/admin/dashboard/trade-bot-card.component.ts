import { Component, EventEmitter, Input, Output, OnInit, OnDestroy } from '@angular/core';
import { Observable } from "rxjs/Rx";
import { TradeBotModel, TradeBotStates } from '../../models/index';
import { ApiService, ToastrService, FeedService } from '../../services/index';

@Component({
    selector: 'trade-bot-card-cmp',
    template: require('./trade-bot-card.component.html'),
    styles: [require('../../app.component.css')],
})

export class TradeBotCardComponent {
    public TradeBotState = TradeBotStates;
    @Input() TradeBot: TradeBotModel;

    constructor(private _api: ApiService, private _toastr: ToastrService) {
        _api.Feed.changeBotState.subscribe(botState => {
            if (this.TradeBot.Id === botState.Id)
                this.TradeBot.State = botState.State;
        });
    }

    public Start() {
        this._api.StartBot(this.TradeBot.Id)
            .subscribe(ok => { });
    }

    public Stop() {
        this._api.StopBot(this.TradeBot.Id).subscribe(r => { });
    }
}
