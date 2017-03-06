import { Injectable } from '@angular/core';
import { Observable } from "rxjs/Rx";
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { PackageModel, PluginModel, ExtBotModel, BotModel, FakeData, BotState, Guid, AccountModel } from "../models/index";
import { Http, Request, Response, RequestOptionsArgs, Headers } from '@angular/http';
import { FeedService } from './feed.service';

@Injectable()
export class ApiService {
    private headers: Headers = new Headers({ 'Content-Type': 'application/json' });
    private repositoryUrl: string = '/api/Repository';


    dasboardBots: Observable<ExtBotModel[]>;
    private _bots: BehaviorSubject<ExtBotModel[]>;
    private dataStore: {
        bots: ExtBotModel[]
    };

    constructor(private http: Http, public feed: FeedService) {
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

    /* >>> API Repository*/
    uploadAlgoPackage(file: any) {
        let input = new FormData();
        input.append("file", file);

        return this.http
            .post(this.repositoryUrl, input);
    }

    deleteAlgoPackage(name: string) {
        return this.http
            .delete(`${this.repositoryUrl}/${name}`, { headers: this.headers });
    }

    getAlgoPackages(): Observable<PackageModel[]> {
        return this.http
            .get(this.repositoryUrl)
            .map(res => res.json().map(i => new PackageModel().Deserialize(i)));
    }
    /* <<< API Repository*/


    /* >>> API Accounts */

    addAccount(acc: AccountModel) {

    }

    getAccounts(): Observable<AccountModel[]> {
        return Observable.of(new AccountModel[0]);
    }

    deleteAccount(acc: AccountModel) {

    }

    updateAccount(acc: AccountModel) {

    }

    /* <<< API Accounts */

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