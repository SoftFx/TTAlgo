import { Injectable } from '@angular/core';
import { Observable } from "rxjs/Rx";
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { PackageModel, PluginModel, ExtBotModel, BotModel, FakeData, BotState, PluginSetupModel, Guid, AccountModel, ResponseStatus, ResponseCode, TradeBotModel, AuthCredentials } from "../models/index";
import { Http, Request, Response, RequestOptionsArgs, RequestOptions, Headers } from '@angular/http';
import { FeedService } from './feed.service';
import { AuthService } from './auth.service';

@Injectable()
export class ApiService {
    private headers: Headers = new Headers({ 'Content-Type': 'application/json' });
    private repositoryUrl: string = '/api/Repository';
    private accountsUrl: string = '/api/Account';

    private readonly testAccountUrl: string = '/api/TestAccount';
    private readonly dashboardUrl: string = '/api/Dashboard';
    private readonly tradeBotUrl: string = '/api/TradeBot';

    constructor(private _http: Http, public Auth: AuthService, public Feed: FeedService) {
        this.Auth.AuthDataUpdated.subscribe(authData => {
            if (authData) {
                this.headers.append('Authorization', 'Bearer ' + authData.Token);
            }
            else {
                this.headers.delete('Authorization');
            }
        });
    }

    GetTradeBots() {
        return this._http.get(this.dashboardUrl, { headers: this.headers })
            .map(res => res.json().map(tb => new TradeBotModel().Deserialize(tb)))
            .catch(this.handleServerError);
    }

    AddBot(setup: PluginSetupModel) {
        return this._http.post(this.dashboardUrl, setup.Payload, { headers: this.headers })
            .map(res => new TradeBotModel().Deserialize(res.json()))
            .catch(this.handleServerError);
    }

    DeleteBot(botId: string) {
        return this._http
            .delete(`${this.dashboardUrl}/?` + $.param({ botId: botId }), { headers: this.headers })
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

        var header = new Headers({ 'Authorization': 'Bearer ' + this.Auth.AuthData.Token });

        return this._http
            .post(this.repositoryUrl, input, { headers: header })
            .catch(this.handleServerError);
    }

    DeletePackage(name: string) {
        return this._http
            .delete(`${this.repositoryUrl}/${name}`, { headers: this.headers })
            .catch(this.handleServerError);
    }

    GetPackages(): Observable<PackageModel[]> {
        return this._http
            .get(this.repositoryUrl, { headers: this.headers })
            .map(res => res.json().map(i => new PackageModel().Deserialize(i)))
            .catch(this.handleServerError);
    }
    /* <<< API Repository*/


    /* >>> API Accounts */
    GetAccounts(): Observable<AccountModel[]> {
        return this._http
            .get(this.accountsUrl, { headers: this.headers })
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

    ChangeAccountPassword(acc: AccountModel) {
        return this._http
            .patch(this.accountsUrl, acc, { headers: this.headers })
            .catch(this.handleServerError);
    }

    TestAccount(acc: AccountModel) {
        return this._http.post(this.testAccountUrl, acc, { headers: this.headers })
            .catch(this.handleServerError);
    }
    /* <<< API Accounts */

    GetSymbols(account: AccountModel) {
        return Observable.of(['EURUSD', 'AEDAUD', 'USDAFN', 'USDAMD']);
    }


    private handleServerError(error: Response): Observable<any> {
        console.error('[ApiService] An error occurred' + error); //debug
        return Observable.throw(new ResponseStatus(error));
    }
}