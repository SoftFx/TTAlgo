﻿import { Injectable, NgZone } from '@angular/core';
import { Http } from '@angular/http';
import 'rxjs/add/operator/toPromise';
import { Observable } from "rxjs/Observable";
import { Subject } from "rxjs/Subject";

import '../../../node_modules/signalr/jquery.signalR.js';

import { FeedSignalR, FeedProxy, FeedServer, FeedClient, ConnectionStatus, PackageModel, AccountModel, TradeBotStateModel, TradeBotModel } from '../models/index';


@Injectable()
export class FeedService {
    currentState = ConnectionStatus.Disconnected;
    connectionState: Observable<ConnectionStatus>;

    addPackage: Observable<PackageModel>;
    deletePackage: Observable<string>;
    addAccount: Observable<AccountModel>;
    deleteAccount: Observable<AccountModel>;
    addBot: Observable<TradeBotModel>;
    deleteBot: Observable<string>;
    updateBot: Observable<TradeBotModel>;

    changeBotState: Observable<TradeBotStateModel>;

    private connectionStateSubject = new Subject<ConnectionStatus>();
    private addPackageSubject = new Subject<PackageModel>();
    private deletePackageSubject = new Subject<string>();
    private addAccountSubject = new Subject<AccountModel>();
    private deleteAccountSubject = new Subject<AccountModel>();
    private changeBotStateSubject = new Subject<TradeBotStateModel>();
    private addBotSubject = new Subject<TradeBotModel>();
    private deleteBotSubject = new Subject<string>();
    private updateBotSubject = new Subject<TradeBotModel>();

    constructor(private _zone: NgZone) {
        this.connectionState = this.connectionStateSubject.asObservable();

        this.deletePackage = this.deletePackageSubject.asObservable();
        this.addPackage = this.addPackageSubject.asObservable();
        this.addAccount = this.addAccountSubject.asObservable();
        this.deleteAccount = this.deleteAccountSubject.asObservable();
        this.changeBotState = this.changeBotStateSubject.asObservable();
        this.addBot = this.addBotSubject.asObservable();
        this.deleteBot = this.deleteBotSubject.asObservable();
        this.updateBot = this.updateBotSubject.asObservable();
    }

    public start(debug: boolean, token?: string): Observable<ConnectionStatus> {
        if (token) {
            $.connection.hub.qs = { 'authorization-token': token };
        }

        $.connection.hub.logging = debug;
        
        let connection = <FeedSignalR>$.connection;
        let feedHub = connection.dSFeed;

        feedHub.client.addPackage = x => this.onAddPackage(new PackageModel().Deserialize(x));
        feedHub.client.deletePackage = x => this.onDeletePackage(x);
        feedHub.client.addAccount = x => this.onAddAccount(new AccountModel().Deserialize(x));
        feedHub.client.deleteAccount = x => this.onDeleteAccount(new AccountModel().Deserialize(x));
        feedHub.client.changeBotState = x => this.onChangeBotState(new TradeBotStateModel().Deserialize(x));
        feedHub.client.addBot = x => this.onBotAdded(new TradeBotModel().Deserialize(x));
        feedHub.client.deleteBot = x => this.onBotDeleted(x);
        feedHub.client.updateBot = x => this.onBotUpdated(new TradeBotModel().Deserialize(x));

        $.connection.hub.start()
            .done(response => this.setConnectionState(ConnectionStatus.Connected))
            .fail(error =>  this._zone.run(() => this.connectionStateSubject.error(error)));

        return this.connectionState;
    }

    public stop(): Observable<ConnectionStatus> {
        $.connection.hub.stop(true, true);
        $.connection.hub.qs = {};
        this.setConnectionState(ConnectionStatus.Disconnected);
        return this.connectionState;
    }

    private setConnectionState(connectionState: ConnectionStatus) {
        this._zone.run(() => {
            console.log('connection state changed to: ' + connectionState);
            this.currentState = connectionState;
            this.connectionStateSubject.next(connectionState);
        });
    }

    private onAddPackage(algoPackage: PackageModel) {
        this._zone.run(() => {
            console.info('[FeedService] onAddPackage', algoPackage);
            this.addPackageSubject.next(algoPackage);
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
            console.info('[FeedService] onChangeBotState', botState);
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