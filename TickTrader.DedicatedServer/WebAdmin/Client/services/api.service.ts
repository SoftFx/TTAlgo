import { Injectable } from '@angular/core';
import { Observable } from "rxjs/Rx";
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { PackageModel, PluginModel, SetupModel, Guid, AccountModel, ResponseStatus, ResponseCode, TradeBotModel, TradeBotLog, AuthCredentials, AccountInfo, TradeBotStatus, File } from "../models/index";
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

    GetDownloadLogUrl(botId: string, file: string) {
        var token = this.Auth.AuthData ? this.Auth.AuthData.Token : "";
        return `${this._tradeBotsUrl}/${encodeURIComponent(botId)}/Logs/${file}?authorization-token=${token}`;
    }

    GetDownloadAlgoDataUrl(botId: string, file: string) {
        var token = this.Auth.AuthData ? this.Auth.AuthData.Token : "";
        return `${this._tradeBotsUrl}/${encodeURIComponent(botId)}/AlgoData/${file}?authorization-token=${token}`;
    }

    AutogenerateBotId(name: string) {
        return this._http.get(`${this._tradeBotsUrl}/` + encodeURIComponent(name) + '/BotId', { headers: this.headers })
            .map(res => res.text())
            .catch(err => this.handleServerError(err));
    }

    GetTradeBot(id: string) {
        return this._http.get(`${this._tradeBotsUrl}/` + encodeURIComponent(id), { headers: this.headers })
            .map(res => new TradeBotModel().Deserialize(res.json()))
            .catch(err => this.handleServerError(err));
    }


    GetTradeBotAlgoData(id: string) {
        return this._http.get(`${this._tradeBotsUrl}/` + encodeURIComponent(id) + '/AlgoData', { headers: this.headers })
            .map(res => res.json().map(f => new File().Deserialize(f)))
            .catch(err => this.handleServerError(err));
    }

    GetTradeBotLog(id: string) {
        return this._http.get(`${this._tradeBotsUrl}/` + encodeURIComponent(id) + '/Logs', { headers: this.headers })
            .map(res => new TradeBotLog().Deserialize(res.json()))
            .catch(err => this.handleServerError(err));
    }

    GetTradeBotStatus(id: string) {
        return this._http.get(`${this._tradeBotsUrl}/` + encodeURIComponent(id) + '/Status', { headers: this.headers })
            .map(res => new TradeBotStatus().Deserialize(res.json()))
            .catch(err => this.handleServerError(err));
    }

    GetTradeBots() {
        return this._http.get(this._tradeBotsUrl, { headers: this.headers })
            .map(res => res.json().map(tb => new TradeBotModel().Deserialize(tb)))
            .catch(err => this.handleServerError(err));
    }

    AddBot(setup: SetupModel) {
        return this._http.post(this._tradeBotsUrl, setup.Payload, { headers: this.headers })
            .map(res => new TradeBotModel().Deserialize(res.json()))
            .catch(err => this.handleServerError(err));
    }

    UpdateBotConfig(botId: string, setup: SetupModel) {
        return this._http.put(`${this._tradeBotsUrl}/` + encodeURIComponent(botId), setup.Payload, { headers: this.headers })
            .catch(err => this.handleServerError(err));
    }

    DeleteBot(botId: string, cleanLog: boolean, cleanAlgoData: boolean) {
        return this._http
            .delete(`${this._tradeBotsUrl}/?` + $.param({ id: botId, clean_log: cleanLog, clean_algodata: cleanAlgoData }), { headers: this.headers })
            .catch(err => this.handleServerError(err));
    }

    DeleteLog(botId: string) {
        return this._http
            .delete(`${this._tradeBotsUrl}/${encodeURIComponent(botId)}/Logs`, { headers: this.headers })
            .catch(err => this.handleServerError(err));
    }

    DeleteLogFile(botId: string, file: string) {
        return this._http
            .delete(`${this._tradeBotsUrl}/${encodeURIComponent(botId)}/Logs/${file}`, { headers: this.headers })
            .catch(err => this.handleServerError(err));
    }

    StartBot(botId: string) {
        return this._http.patch(`${this._tradeBotsUrl}/` + encodeURIComponent(botId) + "/Start", null, { headers: this.headers })
            .catch(err => this.handleServerError(err));
    }

    StopBot(botId: string) {
        return this._http.patch(`${this._tradeBotsUrl}/` + encodeURIComponent(botId) + "/Stop", null, { headers: this.headers })
            .catch(err => this.handleServerError(err));
    }

    /* >>> API Repository*/
    UploadPackage(file: any) {
        let input = new FormData();
        input.append("file", file);

        var header = new Headers({ 'Authorization': 'Bearer ' + this.Auth.AuthData.Token });

        return this._http
            .post(this._packagesUrl, input, { headers: header })
            .catch(err => this.handleServerError(err));
    }

    DeletePackage(name: string) {
        return this._http
            .delete(`${this._packagesUrl}/` + encodeURIComponent(name), { headers: this.headers })
            .catch(err => this.handleServerError(err));
    }

    GetPackages(): Observable<PackageModel[]> {
        return this._http
            .get(this._packagesUrl, { headers: this.headers })
            .map(res => res.json().map(i => new PackageModel().Deserialize(i)))
            .catch(err => this.handleServerError(err));
    }
    /* <<< API Repository*/


    /* >>> API Accounts */
    GetAccountInfo(acc: AccountModel): Observable<AccountInfo> {
        return this._http
            .get(this._accountsUrl + `/${encodeURIComponent(acc.Server)}/${encodeURIComponent(acc.Login)}/Info`, { headers: this.headers })
            .map(res => new AccountInfo().Deserialize(res.json()))
            .catch(err => this.handleServerError(err));
    }

    GetAccounts(): Observable<AccountModel[]> {
        return this._http
            .get(this._accountsUrl, { headers: this.headers })
            .map(res => res.json().map(i => new AccountModel().Deserialize(i)))
            .catch(err => this.handleServerError(err));
    }

    AddAccount(acc: AccountModel) {
        return this._http
            .post(this._accountsUrl, acc, { headers: this.headers })
            .catch(err => this.handleServerError(err));
    }

    DeleteAccount(acc: AccountModel) {
        return this._http
            .delete(`${this._accountsUrl}/?` + $.param({ login: acc.Login, server: acc.Server }), { headers: this.headers })
            .catch(err => this.handleServerError(err));
    }

    ChangeAccountPassword(acc: AccountModel) {
        return this._http
            .patch(this._accountsUrl, acc, { headers: this.headers })
            .catch(err => this.handleServerError(err));
    }

    TestAccount(acc: AccountModel) {
        return this._http.get(`${this._accountsUrl}/Test/?` + $.param({ login: acc.Login, server: acc.Server, password: acc.Password }), { headers: this.headers })
            .catch(err => this.handleServerError(err));
    }
    /* <<< API Accounts */

    private handleServerError(error: Response): Observable<any> {
        console.error('[ApiService] An error occurred' + error); //debug
        let responseErr = new ResponseStatus(error);

        if (responseErr.Status === 401)
            this.Auth.LogOut();

        return Observable.throw(new ResponseStatus(error));
    }
}