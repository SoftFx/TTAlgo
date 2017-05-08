import { Injectable } from '@angular/core';
import { Observable } from "rxjs/Rx";
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { PackageModel, PluginModel, SetupModel, Guid, AccountModel, ResponseStatus, ResponseCode, TradeBotModel, AuthCredentials } from "../models/index";
import { Http, Request, Response, RequestOptionsArgs, RequestOptions, Headers } from '@angular/http';
import { FeedService } from './feed.service';
import { AuthService } from './auth.service';

@Injectable()
export class ApiService {
    private headers: Headers = new Headers({ 'Content-Type': 'application/json' });
    private readonly _packagesUrl: string = '/api/Packages';
    private readonly _accountsUrl: string = '/api/Accounts';
    private readonly _tradeBotsUrl: string = '/api/TradeBots';

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

    AutogenerateBotId(name: string) {
        return this._http.get(`${this._tradeBotsUrl}/` + encodeURIComponent(name) + '/BotId', { headers: this.headers })
            .map(res => res.text())
            .catch(this.handleServerError);
    }

    GetTradeBot(id: string) {
        return this._http.get(`${this._tradeBotsUrl}/` + encodeURIComponent(id), { headers: this.headers })
            .map(res => new TradeBotModel().Deserialize(res.json()))
            .catch(this.handleServerError);
    }

    GetTradeBots() {
        return this._http.get(this._tradeBotsUrl, { headers: this.headers })
            .map(res => res.json().map(tb => new TradeBotModel().Deserialize(tb)))
            .catch(this.handleServerError);
    }

    AddBot(setup: SetupModel) {
        return this._http.post(this._tradeBotsUrl, setup.Payload, { headers: this.headers })
            .map(res => new TradeBotModel().Deserialize(res.json()))
            .catch(this.handleServerError);
    }

    UpdateBotConfig(botId: string, setup: SetupModel) {
        return this._http.put(`${this._tradeBotsUrl}/` + encodeURIComponent(botId), setup.Payload, { headers: this.headers })
            .catch(this.handleServerError);
    }

    DeleteBot(botId: string) {
        return this._http
            .delete(`${this._tradeBotsUrl}/` + encodeURIComponent(botId), { headers: this.headers })
            .catch(this.handleServerError);
    }

    StartBot(botId: string) {
        return this._http.patch(`${this._tradeBotsUrl}/` + encodeURIComponent(botId) + "/Start", null, { headers: this.headers })
            .catch(this.handleServerError);
    }

    StopBot(botId: string) {
        return this._http.patch(`${this._tradeBotsUrl}/` + encodeURIComponent(botId) + "/Stop", null, { headers: this.headers })
            .catch(this.handleServerError);
    }

    /* >>> API Repository*/
    UploadPackage(file: any) {
        let input = new FormData();
        input.append("file", file);

        var header = new Headers({ 'Authorization': 'Bearer ' + this.Auth.AuthData.Token });

        return this._http
            .post(this._packagesUrl, input, { headers: header })
            .catch(this.handleServerError);
    }

    DeletePackage(name: string) {
        return this._http
            .delete(`${this._packagesUrl}/` + encodeURIComponent(name), { headers: this.headers })
            .catch(this.handleServerError);
    }

    GetPackages(): Observable<PackageModel[]> {
        return this._http
            .get(this._packagesUrl, { headers: this.headers })
            .map(res => res.json().map(i => new PackageModel().Deserialize(i)))
            .catch(this.handleServerError);
    }
    /* <<< API Repository*/


    /* >>> API Accounts */
    GetAccounts(): Observable<AccountModel[]> {
        return this._http
            .get(this._accountsUrl, { headers: this.headers })
            .map(res => res.json().map(i => new AccountModel().Deserialize(i)))
            .catch(this.handleServerError);
    }

    AddAccount(acc: AccountModel) {
        return this._http
            .post(this._accountsUrl, acc, { headers: this.headers })
            .catch(this.handleServerError);
    }

    DeleteAccount(acc: AccountModel) {
        return this._http
            .delete(`${this._accountsUrl}/?` + $.param({ login: acc.Login, server: acc.Server }), { headers: this.headers })
            .catch(this.handleServerError);
    }

    ChangeAccountPassword(acc: AccountModel) {
        return this._http
            .patch(this._accountsUrl, acc, { headers: this.headers })
            .catch(this.handleServerError);
    }

    TestAccount(acc: AccountModel) {
        return this._http.get(`${this._accountsUrl}/Test/?` + $.param({ login: acc.Login, server: acc.Server }), { headers: this.headers })
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