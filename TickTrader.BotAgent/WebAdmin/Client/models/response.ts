import { Response } from '@angular/http';
import { Serializable } from './index';

export class ResponseStatus {

    public Code: ResponseCode;
    public Status: number;
    public Message: string;
    public Ok: boolean;

    constructor(response?: Response) {
        if (response)
            this.parse(response);
    }

    public get Handled(): boolean { return this.Code !== -1 }

    private parse(response: Response) {
        try {
            var responseBody = response.json();
            this.Code = responseBody.Code ? responseBody.Code : ResponseCode.None;
            this.Message = responseBody.Message;
            this.Ok = response.ok;
            return;
        }
        catch (ex) { /*normal case*/ }

        this.Code = ResponseCode.None;
        this.Status = response.status;
        this.Ok = response.ok;
        this.Message = response.statusText;
    }
}

export enum ResponseCode {
    None = -1,
    Unknown = 0,
    InvalidCredentials = 100,
    PackageNotFound = 1001,
    PackageIsLocked = 1002,
    DuplicateAccount = 2000,
    AccountNotFound = 2001,
    InvalidAccount = 2002,
    AccountIsLocked = 2003,
    DuplicateBot = 3000,
    BotNotFound = 3001,
    InvalidBot = 3002,
    CommunicationError = 4000,
    InvalidState = 10000
}

export enum ConnectionErrorCodes {
    None,
    Unknown,
    NetworkError,
    Timeout,
    BlockedAccount,
    ClientInitiated,
    InvalidCredentials,
    SlowConnection,
    ServerError,
    LoginDeleted,
    ServerLogout,
    Canceled
}

export class ConnectionErrorInfo {

    public Code: ConnectionErrorCodes;

    public TextMessage: string;
}

export class ConnectionTestResult {

    public ErrorInfo: ConnectionErrorInfo;

    constructor(info: ConnectionErrorInfo) {
        this.ErrorInfo = info;
    }

    public get Message(): string {
        switch (this.ErrorInfo.Code) {
            case ConnectionErrorCodes.Unknown:
                return this.ErrorInfo.TextMessage === undefined || this.ErrorInfo.TextMessage === "" ? "Unknown error" : this.ErrorInfo.TextMessage;
            case ConnectionErrorCodes.NetworkError:
                return "Network error";
            case ConnectionErrorCodes.Timeout:
                return "Timeout";
            case ConnectionErrorCodes.BlockedAccount:
                return "Blocked account";
            case ConnectionErrorCodes.InvalidCredentials:
                return "Invalid credentials";
            case ConnectionErrorCodes.SlowConnection:
                return "Slow connection";
            case ConnectionErrorCodes.ServerError:
                return "Server error";
            case ConnectionErrorCodes.LoginDeleted:
                return "Login deleted";
            case ConnectionErrorCodes.ServerLogout:
                return "Server logout";
            case ConnectionErrorCodes.Canceled:
                return "Canceled";
            default: return "Connection success";
        }
    }

    public get TestPassed(): boolean {
        console.log(this.ErrorInfo);
        switch (this.ErrorInfo.Code) {
            case ConnectionErrorCodes.Unknown:
            case ConnectionErrorCodes.NetworkError:
            case ConnectionErrorCodes.Timeout:
            case ConnectionErrorCodes.BlockedAccount:
            case ConnectionErrorCodes.InvalidCredentials:
            case ConnectionErrorCodes.SlowConnection:
            case ConnectionErrorCodes.ServerError:
            case ConnectionErrorCodes.LoginDeleted:
            case ConnectionErrorCodes.ServerLogout:
            case ConnectionErrorCodes.Canceled:
                return false;
            default: return true;
        }
    }
}

