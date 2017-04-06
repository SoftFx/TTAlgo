import { Injectable } from '@angular/core';
import { Http, Request, Response, RequestOptionsArgs, Headers } from '@angular/http';
import { AuthData, AuthCredentials, Utils } from '../models/index';
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

        this.restoreAuthData();
    }

    public IsAuthorized(): boolean {
        if (localStorage.getItem(this._storageKey)) {
            let authData = <AuthData>JSON.parse(localStorage.getItem(this._storageKey), Utils.DateReviver);

            let nowUtc = new Date(new Date().toUTCString());
            if (authData.expires >= nowUtc)
                return true;
        }
        return false;
    }

    public get AuthData(): AuthData {
        let authData = <AuthData>JSON.parse(localStorage.getItem(this._storageKey), Utils.DateReviver);

        return authData;
    }

    public LogIn(login: string, password: string): Observable<boolean> {
        return this._http.post(this._loginUrl, { Login: login, Password: password })
            .map((response: Response) => response.json())
            .do(authData => {
                if (authData && authData.token) {
                    localStorage.setItem(this._storageKey, JSON.stringify(authData));

                    this._authDataUpdatedSubject.next(<AuthData>authData);

                    this._feed.start(true).subscribe(null, error => console.log('Error on init: ' + error));

                    return Observable.of(true);
                }
                return Observable.of(false);
            });
    }

    public LogOut() {
        this._feed.stop().subscribe(null, error => console.log('Error on init: ' + error));
        localStorage.removeItem(this._storageKey);
        this._authDataUpdatedSubject.next(<AuthData>null);
    }

    private restoreAuthData() {
        let authData = <AuthData>JSON.parse(localStorage.getItem(this._storageKey), Utils.DateReviver);
        this._authDataUpdatedSubject.next(authData);
    }
}