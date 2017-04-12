export class AuthCredentials {
    login: string;
    password: string;

    constructor(login: string = "", password: string = "") {
        this.login = login;
        this.password = password;
    }
}

export class AuthData {
    token: string;
    expires: Date;
    user: string;

    constructor(token: string, expires: string, user: string) {
        this.token = token;
        this.expires = new Date(expires);
        this.user = user;
    }

    IsEmpty(): Boolean {
        return !this.token;
    }
}