import { Injectable } from '@angular/core';
import { Http, Request, Response, RequestOptionsArgs, Headers } from '@angular/http';
import { Observable } from "rxjs/Rx";


@Injectable()
export class AuthService {
    private readonly storageKey = 'a-token';
    public isLoggedIn: boolean = false;

    redirectUrl: string;

    constructor(private http: Http) { }

    isAuthorized(): boolean {
        return this.isLoggedIn;
    }

    logIn(username: string, password: string): Observable<boolean> {
        return Observable.of(true).delay(500).do(val => this.isLoggedIn = true);
    }

    logOut() {
        this.isLoggedIn = false;
    }
}