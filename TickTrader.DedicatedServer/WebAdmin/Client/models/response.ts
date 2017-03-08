import { Response } from '@angular/http';

export class ResponseStatus {
    constructor(response?: Response) {
        if (response)
            this.parse(response);
    }

    public get Handled(): boolean { return this.Code !== -1 }
    public Code: ResponseCode;
    public Status: number;
    public Message: string;
    public Ok: boolean;

    private parse(response: Response) {
        try {
            var responseBody = response.json();
            this.Code = responseBody.Code;
            this.Message = responseBody.Message;
            this.Ok = response.ok;
        }
        catch (ex) { /*normal case*/ }

        this.Code = -1;
        this.Status = response.status;
        this.Ok = response.ok;
        this.Message = response.statusText;
    }
}

export enum ResponseCode {
    DublicatePackage = 101
}

