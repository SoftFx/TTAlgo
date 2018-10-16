import { Injectable, NgZone } from '@angular/core';
import { Http } from '@angular/http';
import 'rxjs/add/operator/toPromise';
import { Observable } from "rxjs/Observable";
import { Subject } from "rxjs/Subject";

import '../../../node_modules/signalr/jquery.signalR.js';

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
            if (token) {
                $.connection.hub.qs = { 'authorization-token': token };
            }

            $.connection.hub.logging = debug;

            let connection = <FeedSignalR>$.connection;
            let feedHub = connection.bAFeed;

            feedHub.client.addOrUpdatePackage = x => this.onAddOrUpdatePackage(new PackageModel().Deserialize(x));
            feedHub.client.deletePackage = x => this.onDeletePackage(x);
            feedHub.client.addAccount = x => this.onAddAccount(new AccountModel().Deserialize(x));
            feedHub.client.deleteAccount = x => this.onDeleteAccount(new AccountModel().Deserialize(x));
            feedHub.client.changeBotState = x => this.onChangeBotState(new TradeBotStateModel().Deserialize(x));
            feedHub.client.addBot = x => this.onBotAdded(new TradeBotModel().Deserialize(x));
            feedHub.client.deleteBot = x => this.onBotDeleted(x);
            feedHub.client.updateBot = x => this.onBotUpdated(new TradeBotModel().Deserialize(x));

            $.connection.hub.disconnected(() => {
                this.setConnectionState(ConnectionStatus.Disconnected);
                if (this.reconnectFlag)
                    this.reconnectAnyway();
            });
            $.connection.hub.reconnecting(() => this.setConnectionState(ConnectionStatus.Reconnecting));
            $.connection.hub.reconnected(() => this.setConnectionState(ConnectionStatus.Connected));
            $.connection.hub.start()
                .done(response => this.setConnectionState(ConnectionStatus.Connected))
                .fail(error => this._zone.run(() => this.connectionStateSubject.error(error)));

            this.reconnectFlag = true;
        }

        return this.ConnectionState;
    }

    public stop(): Observable<ConnectionStatus> {
        if (this.CurrentState !== ConnectionStatus.Disconnected) {

            this.reconnectFlag = false;
            $.connection.hub.stop(true, true);
            $.connection.hub.qs = {};
            this.setConnectionState(ConnectionStatus.Disconnected);
        }
        return this.ConnectionState;
    }

    private reconnectAnyway() {
        setTimeout(() => {
            if (this.reconnectFlag && this.CurrentState === ConnectionStatus.Disconnected) {
                $.connection.hub.start()
                    .done(response => this.setConnectionState(ConnectionStatus.Connected))
                    .fail(error => this.reconnectAnyway());
            }
        }, 10000);
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