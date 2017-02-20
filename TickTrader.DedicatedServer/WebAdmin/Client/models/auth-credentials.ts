export class AuthCredentials {
    login: string;
    password: string;

    constructor(login: string = "", password: string = "") {
        this.login = login;
        this.password = password;
    }
}