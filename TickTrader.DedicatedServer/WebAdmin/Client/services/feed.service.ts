import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import 'rxjs/add/operator/toPromise';
import { Observable } from "rxjs/Observable";
import { Subject } from "rxjs/Subject";

import '../../../node_modules/signalr/jquery.signalR.js';

import { FeedSignalR, FeedProxy, FeedServer, FeedClient, ConnectionStatus, PackageModel, AccountModel, TradeBotStateModel } from '../models/index';


@Injectable()
export class FeedService {
    currentState = ConnectionStatus.Disconnected;
    connectionState: Observable<ConnectionStatus>;

    deletePackage: Observable<string>;
    addPackage: Observable<PackageModel>;
    addAccount: Observable<AccountModel>;
    deleteAccount: Observable<AccountModel>;
    changeBotState: Observable<TradeBotStateModel>;

    private connectionStateSubject = new Subject<ConnectionStatus>();

    private deletePackageSubject = new Subject<string>();
    private addPackageSubject = new Subject<PackageModel>();
    private addAccountSubject = new Subject<AccountModel>();
    private deleteAccountSubject = new Subject<AccountModel>();
    private changeBotStateSubject = new Subject<TradeBotStateModel>();

    constructor(private _http: Http) {
        this.connectionState = this.connectionStateSubject.asObservable();

        this.deletePackage = this.deletePackageSubject.asObservable();
        this.addPackage = this.addPackageSubject.asObservable();
        this.addAccount = this.addAccountSubject.asObservable();
        this.deleteAccount = this.deleteAccountSubject.asObservable();
        this.changeBotState = this.changeBotStateSubject.asObservable();
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

        $.connection.hub.start()
            .done(response => this.setConnectionState(ConnectionStatus.Connected))
            .fail(error => this.connectionStateSubject.error(error));

        return this.connectionState;
    }

    public stop(): Observable<ConnectionStatus> {
        $.connection.hub.stop(true, true);
        $.connection.hub.qs = {};
        this.setConnectionState(ConnectionStatus.Disconnected);
        return this.connectionState;
    }

    private setConnectionState(connectionState: ConnectionStatus) {
        console.log('connection state changed to: ' + connectionState);
        this.currentState = connectionState;
        this.connectionStateSubject.next(connectionState);
    }

    private onAddPackage(algoPackage: PackageModel) {
        console.info('[FeedService] onAddPackage', algoPackage);
        this.addPackageSubject.next(algoPackage);
    }

    private onDeletePackage(name: string) {
        console.info('[FeedService] onDeletePackage', name);
        this.deletePackageSubject.next(name);
    }

    private onAddAccount(account: AccountModel) {
        console.info('[FeedService] onAddAccount', account);
        this.addAccountSubject.next(account);
    }

    private onDeleteAccount(account: AccountModel) {
        console.info('[FeedService] onDeleteAccount', account);
        this.deleteAccountSubject.next(account);
    }

    private onChangeBotState(botState: TradeBotStateModel) {
        console.info('[FeedService] onChangeBotState', botState);
        this.changeBotStateSubject.next(botState);
    }
}