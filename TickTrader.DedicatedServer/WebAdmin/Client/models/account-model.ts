import { Serializable } from './index';

export class AccountModel implements Serializable<AccountModel> {
    public Login: string = "";
    public Server: string = "";
    public Password: string = "";

    constructor(){}

    public Deserialize(input: any): AccountModel {
        this.Login = input.Login;
        this.Server = input.Server;

        return this;
    }
}