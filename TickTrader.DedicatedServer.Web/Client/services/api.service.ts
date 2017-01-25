import { Injectable } from '@angular/core';
import { Observable } from "rxjs/Rx";
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { ExtBotModel, BotModel, FakeData, BotState, Guid } from "../models/index";

@Injectable()
export class ApiService {
    dasboardBots: Observable<ExtBotModel[]>;
    private _bots: BehaviorSubject<ExtBotModel[]>;
    private dataStore: {
        bots: ExtBotModel[]
    };

    constructor() {
        this.dataStore = { bots: [] };
        this._bots = <BehaviorSubject<ExtBotModel[]>>new BehaviorSubject([]);
        this.dasboardBots = this._bots.asObservable();
    }

    loadAllBots(): Observable<BotModel[]> {
        return Observable.of(FakeData.bots);
    }

    loadBotsOnDashboard() {
        Observable.of(FakeData.extBots).delay(150).subscribe(data => {
            this.dataStore.bots = data;
            this._bots.next(Object.assign({}, this.dataStore).bots);
        }, error => console.log('Could not load bots.'));
    }

    getBot(id: string): Observable<ExtBotModel> {
        return Observable.of(this.dataStore.bots.find(bot => bot.instanceId == id));
    }

    removeBotFromDashboard(bot: ExtBotModel) {
        Observable.of(true).delay(150).subscribe(response => {
            this.dataStore.bots.forEach((b, i) => {
                if (b == bot) { this.dataStore.bots.splice(i, 1); }
            });

            this._bots.next(Object.assign({}, this.dataStore).bots);
        }, error => console.log('Could not delete bot.'));
    }

    addBot(bot: ExtBotModel) {
        return Observable
            .of(true)
            .delay(150)
            .do(response => {
                this.dataStore.bots.push(bot);
            },
            err => { },
            () => { });
    }

    runBot(bot: ExtBotModel) {
        return Observable.from([BotState.Running, BotState.Runned])
            .map(function (value) { return Observable.of(value).delay(value == BotState.Running ? 0 : 1500); })
            .concatAll()
            .subscribe(response => {
                this.updateBotState(bot, response);
            },
            err => { },
            () => { });
    }

    stopBot(bot: ExtBotModel) {
        return Observable.from([BotState.Stopping, BotState.Stopped])
            .map(function (value) { return Observable.of(value).delay(value == BotState.Stopping ? 0 : 1500); })
            .concatAll()
            .subscribe(response => {
                this.updateBotState(bot, response);
            },
            err => { },
            () => { });
    }

    private updateBotState(bot: ExtBotModel, state: BotState): boolean {
        for (let cBot of this.dataStore.bots) {
            if (cBot.instanceId == bot.instanceId) {
                cBot.state = state;
                return true;
            }
        }
        return false;
    }
}