﻿import { Injectable } from '@angular/core';
import { Http, Request, Response, RequestOptionsArgs, Headers } from '@angular/http';
import { AuthData, AuthCredentials, Utils, ResponseStatus } from '../models/index';
import { Observable } from "rxjs/Rx";
import { FeedService } from './feed.service';
import { Subject } from "rxjs/Subject";
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

@Injectable()
export class AuthService {
    private readonly _storageKey = 'a-token';
    private readonly _loginUrl: string = '/api/Login';
    private _authDataUpdatedSubject = new BehaviorSubject<AuthData>(<AuthData>null);

    public RedirectUrl: string;
    public AuthDataUpdated: Observable<AuthData>;

    constructor(private _http: Http, private _feed: FeedService) {
        this.AuthDataUpdated = this._authDataUpdatedSubject.asObservable();
        this.AuthDataUpdated.subscribe(authData => {
            if (authData && this.IsAuthorized) {
                this._feed.start(true, authData.Token).subscribe(null, error => console.log('Error on init: ' + error));
            }
        });

        this.restoreAuthData();
    }

    public get IsAuthorized(): boolean {
        if (localStorage.getItem(this._storageKey)) {
            let authData = <AuthData>JSON.parse(localStorage.getItem(this._storageKey), Utils.DateReviver);

            let nowUtc = new Date(new Date().toUTCString());
            if (authData.Expires >= nowUtc)
                return true;
        }
        return false;
    }

    public get AuthData(): AuthData {
        let authData = <AuthData>JSON.parse(localStorage.getItem(this._storageKey), Utils.DateReviver);

        return authData;
    }

    public LogIn(login: string, password: string, secretCode: string) {
        return this._http.post(this._loginUrl, { Login: login, Password: password, SecretCode: secretCode })
            .map((response: Response) => new AuthData().Deserialize(response.json()))
            .do(authData => {
                if (authData && authData.Token) {
                    localStorage.setItem(this._storageKey, JSON.stringify(authData));
                    this._authDataUpdatedSubject.next(authData);
                }
                else {
                    console.error('Failed to extract token');
                }
            })
            .catch(this.handleServerError);
    }

    public LogOut() {
        this._feed.stop().subscribe(null, error => console.log('Error on init: ' + error));

        localStorage.removeItem(this._storageKey);

        this.RedirectUrl = "";
        this._authDataUpdatedSubject.next(<AuthData>null);
    }

    private restoreAuthData() {
        let authData = <AuthData>JSON.parse(localStorage.getItem(this._storageKey), Utils.DateReviver);

        if (this.IsAuthorized) {
            this._authDataUpdatedSubject.next(authData);
        }
        else {
            localStorage.removeItem(this._storageKey);
        }
    }

    private handleServerError(error: Response): Observable<any> {
        console.error('[AuthService] An error occurred' + error); //debug
        return Observable.throw(new ResponseStatus(error));
    }
}