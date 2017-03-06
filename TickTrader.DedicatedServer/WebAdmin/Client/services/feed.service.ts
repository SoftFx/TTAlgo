import { Injectable } from '@angular/core';
import { Http } from '@angular/http';

import 'rxjs/add/operator/toPromise';
import { Observable } from "rxjs/Observable";
import { Subject } from "rxjs/Subject";

import '../../../node_modules/signalr/jquery.signalR.js';
$.getScript('signalr/hubs');

import { FeedSignalR, FeedProxy, FeedServer, FeedClient, ConnectionStatus, PackageModel } from '../models/index';

@Injectable()
export class FeedService {

    currentState = ConnectionStatus.Disconnected;
    connectionState: Observable<ConnectionStatus>;

    deletePackage: Observable<string>;
    addPackage: Observable<PackageModel>;

    private connectionStateSubject = new Subject<ConnectionStatus>();

    private deletePackageSubject = new Subject<string>();
    private addPackageSubject = new Subject<PackageModel>();

    private server: FeedServer;

    constructor(private http: Http) {
        this.connectionState = this.connectionStateSubject.asObservable();

        this.deletePackage = this.deletePackageSubject.asObservable();
        this.addPackage = this.addPackageSubject.asObservable();
    }

    public start(debug: boolean): Observable<ConnectionStatus> {
        $.connection.hub.logging = debug;

        let connection = <FeedSignalR>$.connection;
        let feedHub = connection.dSFeed;
        this.server = feedHub.server;

        feedHub.client.addPackage = x => this.onAddPackage(x);
        feedHub.client.deletePackage = x => this.onDeletePackage(x);

        $.connection.hub.start()
            .done(response => this.setConnectionState(ConnectionStatus.Connected))
            .fail(error => this.connectionStateSubject.error(error));

        return this.connectionState;
    }

    public stop(): Observable<ConnectionStatus> {
        $.connection.hub.stop(true, true);
        this.setConnectionState(ConnectionStatus.Disconnected);
        return this.connectionState;
    }

    private setConnectionState(connectionState: ConnectionStatus) {
        console.log('connection state changed to: ' + connectionState);
        this.currentState = connectionState;
        this.connectionStateSubject.next(connectionState);
    }

    private onAddPackage(algoPackage: PackageModel) {
        console.log(algoPackage.Name);
        this.addPackageSubject.next(algoPackage);
    }

    private onDeletePackage(name: string) {
        this.deletePackageSubject.next(name);
    }
}