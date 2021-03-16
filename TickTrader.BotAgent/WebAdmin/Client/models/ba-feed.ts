import { PackageModel, AccountModel, TradeBotStateModel, TradeBotModel } from './ba-models';

export interface FeedProxy {
    client: FeedClient;
    server: FeedServer;
}

export interface FeedClient {
    deletePackage: (packageName: string) => void;
    addOrUpdatePackage: (algoPackage: PackageModel) => void;
    deleteAccount: (account: AccountModel) => void;
    addAccount: (account: AccountModel) => void;
    changeBotState: (state: TradeBotStateModel) => void;
    addBot: (bot: TradeBotModel) => void;
    deleteBot: (botId: string) => void;
    updateBot: (bot: TradeBotModel) => void;
}

export interface FeedServer {

}

export enum ConnectionStatus {
    Connected = 1,
    Disconnected = 2,
    WaitReconnect = 3,
    Reconnecting = 4,
    Error = 5
}