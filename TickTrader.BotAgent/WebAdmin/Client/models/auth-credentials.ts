import { Serializable } from './index';

export class AuthCredentials {
    Login: string;
    Password: string;

    constructor(login: string = "", password: string = "") {
        this.Login = login;
        this.Password = password;
    }
}

export class AuthData implements Serializable<AuthData>{
    Token: string;
    Expires: Date;
    User: string;

    public Deserialize(input: any): AuthData {
        if (input) {
            this.Token = input.Token;
            this.Expires = input.Expires;
            this.User = input.User;
        }

        return this;
    }

    IsEmpty(): Boolean {
        return !this.Token;
    }
}