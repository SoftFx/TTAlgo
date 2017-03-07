import { Injectable } from '@angular/core';
import { Http, Request, Response, RequestOptionsArgs, Headers } from '@angular/http';
import { Observable } from "rxjs/Rx";
import { FeedService } from './feed.service';


@Injectable()
export class AuthService {
    private readonly storageKey = 'a-token';
    public isLoggedIn: boolean = false;

    redirectUrl: string;

    constructor(private feed: FeedService, private http: Http) { }

    isAuthorized(): boolean {
        return this.isLoggedIn;
    }

    logIn(username: string, password: string): Observable<boolean> {
        this.feed.start(true)
            .subscribe(null, error => console.log('Error on init: ' + error));
        return Observable.of(true).delay(500).do(val => this.isLoggedIn = true);
    }

    logOut() {
        this.feed.stop()
            .subscribe(null, error => console.log('Error on init: ' + error));
        return Observable.of(true).do(v => this.isLoggedIn = false);
    }
}