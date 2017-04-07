import { Serializable } from './index';
import { AccountModel } from './account-model';

export class TradeBotModel implements Serializable<TradeBotModel>{
    public Id: string;
    public Status: string;
    public Account: AccountModel;
    public State: TradeBotStates;

    constructor() { }


    public Deserialize(input: any): TradeBotModel {
        this.Id = input.Id;
        this.Status = input.Status;
        this.Account = new AccountModel().Deserialize(input.Account);
        this.State = TradeBotStates[input.State as string];

        return this;
    }
}

export class TradeBotStateModel implements Serializable<TradeBotStateModel>{
    public Id: string;
    public State: TradeBotStates;

    constructor() { }


    public Deserialize(input: any): TradeBotStateModel {
        this.Id = input.Id;
        this.State = TradeBotStates[input.State as string];

        return this;
    }
}

export enum TradeBotStates { Offline, Started, Initializing, Faulted, Online, Stopping }