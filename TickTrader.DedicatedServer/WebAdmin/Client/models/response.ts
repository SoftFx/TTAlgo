export class ResponseStatus
{
    public code: ResponseCode;
    public message: string;
}

export enum ResponseCode {
    DublicatePackage = 101
}

