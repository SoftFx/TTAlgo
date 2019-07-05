import { Injectable, NgZone } from '@angular/core';
import { Http } from '@angular/http';
import 'rxjs/add/operator/toPromise';
import { Observable } from "rxjs/Observable";
import { Subject } from "rxjs/Subject";

import * as signalR from '@aspnet/signalr';

import { FeedSignalR, FeedProxy, FeedServer, FeedClient, ConnectionStatus, PackageModel, AccountModel, TradeBotStateModel, TradeBotModel } from '../models/index';
import { setTimeout } from 'timers';


@Injectable()
export class FeedService {
    public CurrentState = ConnectionStatus.Disconnected;
    public ConnectionState: Observable<ConnectionStatus>;

    public AddOrUpdatePackage: Observable<PackageModel>;
    public DeletePackage: Observable<string>;
    public AddAccount: Observable<AccountModel>;
    public DeleteAccount: Observable<AccountModel>;
    public AddBot: Observable<TradeBotModel>;
    public DeleteBot: Observable<string>;
    public UpdateBot: Observable<TradeBotModel>;
    public ChangeBotState: Observable<TradeBotStateModel>;

    private connectionStateSubject = new Subject<ConnectionStatus>();
    private addOrUpdatePackageSubject = new Subject<PackageModel>();
    private deletePackageSubject = new Subject<string>();
    private addAccountSubject = new Subject<AccountModel>();
    private deleteAccountSubject = new Subject<AccountModel>();
    private changeBotStateSubject = new Subject<TradeBotStateModel>();
    private addBotSubject = new Subject<TradeBotModel>();
    private deleteBotSubject = new Subject<string>();
    private updateBotSubject = new Subject<TradeBotModel>();
    private reconnectFlag: boolean;
    private connection: signalR.HubConnection;

    constructor(private _zone: NgZone) {
        this.ConnectionState = this.connectionStateSubject.asObservable();

        this.DeletePackage = this.deletePackageSubject.asObservable();
        this.AddOrUpdatePackage = this.addOrUpdatePackageSubject.asObservable();
        this.AddAccount = this.addAccountSubject.asObservable();
        this.DeleteAccount = this.deleteAccountSubject.asObservable();
        this.ChangeBotState = this.changeBotStateSubject.asObservable();
        this.AddBot = this.addBotSubject.asObservable();
        this.DeleteBot = this.deleteBotSubject.asObservable();
        this.UpdateBot = this.updateBotSubject.asObservable();
    }

    public start(debug: boolean, token?: string): Observable<ConnectionStatus> {
        if (this.CurrentState !== ConnectionStatus.Connected) {

            let connectionBuilder = new signalR.HubConnectionBuilder();

            if (token) {
                connectionBuilder.withUrl("/signalr", { accessTokenFactory: () => token });
            }
            else {
                connectionBuilder.withUrl("/signalr");
            }

            if (debug) {
                //connectionBuilder.configureLogging(signalR.LogLevel.Debug);
            }

            this.connection = connectionBuilder.build();

            this.connection.on("addOrUpdatePackage", x => this.onAddOrUpdatePackage(new PackageModel().Deserialize(x)));
            this.connection.on("deletePackage", x => this.onDeletePackage(x));
            this.connection.on("addAccount", x => this.onAddAccount(new AccountModel().Deserialize(x)));
            this.connection.on("deleteAccount", x => this.onDeleteAccount(new AccountModel().Deserialize(x)));
            this.connection.on("changeBotState", x => this.onChangeBotState(new TradeBotStateModel().Deserialize(x)));
            this.connection.on("addBot", x => this.onBotAdded(new TradeBotModel().Deserialize(x)));
            this.connection.on("deleteBot", x => this.onBotDeleted(x));
            this.connection.on("updateBot", x => this.onBotUpdated(new TradeBotModel().Deserialize(x)));

            this.connection.onclose(() => {
                this.setConnectionState(ConnectionStatus.Disconnected);
                if (this.reconnectFlag)
                    this.reconnectAnyway();
            });
            this.connection.start()
                .then(response => this.setConnectionState(ConnectionStatus.Connected))
                .catch(error => this._zone.run(() => this.connectionStateSubject.error(error)));

            this.reconnectFlag = true;
        }

        return this.ConnectionState;
    }

    public stop(): Observable<ConnectionStatus> {
        if (this.CurrentState !== ConnectionStatus.Disconnected) {

            this.reconnectFlag = false;
            this.connection.stop();
            this.connection = null;
            this.setConnectionState(ConnectionStatus.Disconnected);
        }
        return this.ConnectionState;
    }

    private reconnectAnyway() {
        if (this.CurrentState !== ConnectionStatus.WaitReconnect) {

            this.setConnectionState(ConnectionStatus.WaitReconnect);
            setTimeout(() => {
                if (this.reconnectFlag && this.CurrentState === ConnectionStatus.WaitReconnect) {
                    this.setConnectionState(ConnectionStatus.Reconnecting);
                    this.connection.start()
                        .then(response => this.setConnectionState(ConnectionStatus.Connected))
                        .catch(error => this.reconnectAnyway());
                }
            }, 10000);
        }
    }

    private setConnectionState(connectionState: ConnectionStatus) {
        if (this.CurrentState == connectionState)
            return;

        this._zone.run(() => {
            console.log('[FeedService] ConnectionState: ', ConnectionStatus[connectionState]);
            this.CurrentState = connectionState;
            this.connectionStateSubject.next(connectionState);
        });
    }

    private onAddOrUpdatePackage(algoPackage: PackageModel) {
        this._zone.run(() => {
            console.info('[FeedService] onAddOrUpdatePackage', algoPackage);
            this.addOrUpdatePackageSubject.next(algoPackage);
        });
    }

    private onDeletePackage(name: string) {
        this._zone.run(() => {
            console.info('[FeedService] onDeletePackage', name);
            this.deletePackageSubject.next(name);
        });
    }

    private onAddAccount(account: AccountModel) {
        this._zone.run(() => {
            console.info('[FeedService] onAddAccount', account);
            this.addAccountSubject.next(account);
        });
    }

    private onDeleteAccount(account: AccountModel) {
        this._zone.run(() => {
            console.info('[FeedService] onDeleteAccount', account);
            this.deleteAccountSubject.next(account);
        });
    }

    private onChangeBotState(botState: TradeBotStateModel) {
        this._zone.run(() => {
            console.info('[FeedService] onChangeBotState -> ' + botState.toString());
            this.changeBotStateSubject.next(botState);
        });
    }

    private onBotAdded(bot: TradeBotModel) {
        this._zone.run(() => {
            console.info('[FeedService] onBotAdded', bot);
            this.addBotSubject.next(bot);
        });
    }

    private onBotDeleted(botId: string) {
        this._zone.run(() => {
            console.info('[FeedService] onBotDeleted', botId);
            this.deleteBotSubject.next(botId);
        });
    }

    private onBotUpdated(bot: TradeBotModel) {
        this._zone.run(() => {
            console.info('[FeedService] onBotUpdated', bot);
            this.updateBotSubject.next(bot);
        });
    }
}