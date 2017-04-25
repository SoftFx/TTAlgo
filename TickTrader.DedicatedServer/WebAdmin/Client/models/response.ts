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
    DuplicatePackage = 1000,
    DuplicateAccount = 2000,

    InvalidState = 3000
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

export class ConnectionTestResult {

    public ConnectionErrorCode: ConnectionErrorCodes;

    constructor(code: ConnectionErrorCodes) {
        this.ConnectionErrorCode = code;
    }

    public get Message(): string {
        switch (this.ConnectionErrorCode) {
            case ConnectionErrorCodes.Unknown:
            case ConnectionErrorCodes.NetworkError:
                return "Network Error";
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
        switch (this.ConnectionErrorCode) {
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

