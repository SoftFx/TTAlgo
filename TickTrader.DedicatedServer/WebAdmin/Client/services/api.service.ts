import { Injectable } from '@angular/core';
import { Observable } from "rxjs/Rx";
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { PackageModel, PluginModel, ExtBotModel, BotModel, FakeData, BotState, PluginSetupModel, Guid, AccountModel, ResponseStatus, ResponseCode, TradeBotModel } from "../models/index";
import { Http, Request, Response, RequestOptionsArgs, Headers } from '@angular/http';
import { FeedService } from './feed.service';

@Injectable()
export class ApiService {
    private headers: Headers = new Headers({ 'Content-Type': 'application/json' });
    private repositoryUrl: string = '/api/Repository';
    private accountsUrl: string = '/api/Account';
    private testAccountUrl: string = '/api/TestAccount';
    private dashboardUrl: string = '/api/Dashboard';
    private tradeBotUrl: string = '/api/TradeBot';


    dasboardBots: Observable<ExtBotModel[]>;
    private _bots: BehaviorSubject<ExtBotModel[]>;
    private dataStore: {
        bots: ExtBotModel[]
    };

    constructor(private _http: Http, public Feed: FeedService) {
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

    GetTradeBots() {
        return this._http.get(this.dashboardUrl)
            .map(res => res.json().map(tb => new TradeBotModel().Deserialize(tb)))
            .catch(this.handleServerError);
    }

    SetupPlugin(setup: PluginSetupModel) {
        return this._http.post(this.dashboardUrl, setup.Payload, { headers: this.headers })
            .map(res => new TradeBotModel().Deserialize(res.json()))
            .catch(this.handleServerError);
    }

    StartBot(botId: string) {
        return this._http.post(this.tradeBotUrl, { Command: "start", BotId: botId }, { headers: this.headers })
            .catch(this.handleServerError);
    }

    StopBot(botId: string) {
        return this._http.post(this.tradeBotUrl, { Command: "stop", BotId: botId }, { headers: this.headers })
            .catch(this.handleServerError);
    }

    /* >>> API Repository*/
    UploadPackage(file: any) {
        let input = new FormData();
        input.append("file", file);

        return this._http
            .post(this.repositoryUrl, input)
            .catch(this.handleServerError);
    }

    DeletePackage(name: string) {
        return this._http
            .delete(`${this.repositoryUrl}/${name}`, { headers: this.headers })
            .catch(this.handleServerError);
    }

    GetPackages(): Observable<PackageModel[]> {
        return this._http
            .get(this.repositoryUrl)
            .map(res => res.json().map(i => new PackageModel().Deserialize(i)))
            .catch(this.handleServerError);
    }
    /* <<< API Repository*/


    /* >>> API Accounts */
    GetAccounts(): Observable<AccountModel[]> {
        return this._http
            .get(this.accountsUrl)
            .map(res => res.json().map(i => new AccountModel().Deserialize(i)))
            .catch(this.handleServerError);
    }

    AddAccount(acc: AccountModel) {
        return this._http
            .post(this.accountsUrl, acc, { headers: this.headers })
            .catch(this.handleServerError);
    }

    DeleteAccount(acc: AccountModel) {
        return this._http
            .delete(`${this.accountsUrl}/?` + $.param({ login: acc.Login, server: acc.Server }), { headers: this.headers })
            .catch(this.handleServerError);
    }

    UpdateAccount(acc: AccountModel) {
        return Observable.throw('NotImplemented');
    }

    TestAccount(acc: AccountModel) {
        return this._http.post(this.testAccountUrl, acc, { headers: this.headers })
            .catch(this.handleServerError);
    }
    /* <<< API Accounts */

    GetSymbols(account: AccountModel) {
        return Observable.of(['EURUSD', 'AEDAUD', 'USDAFN', 'USDAMD']);
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


    private handleServerError(error: Response): Observable<any> {
        console.error('[ApiService] An error occurred' + error); //debug
        return Observable.throw(new ResponseStatus(error));
    }
}