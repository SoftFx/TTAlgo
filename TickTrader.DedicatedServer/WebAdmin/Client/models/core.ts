import { Observable } from "rxjs/Rx";
import { ResponseStatus } from './response';

export class ObservableRequest<T>
{
    private _task: Observable<T>;
    private _success: (value: T) => void;
    private _error: (error: ResponseStatus) => void;
    private _complete: () => void;

    public IsRunning: boolean;
    public IsCompleted: boolean;
    public IsFaulted: boolean;
    public Error: ResponseStatus;
    public Result: T;

    constructor(task: Observable<T>) {
        this._task = task;

        this._success = (v) => { };
        this._error = (e) => { };
        this._complete = () => { };

        this.IsRunning = false;
        this.IsCompleted = false;
        this.IsFaulted = false;
    }

    public Subscribe(ok?: (value: T) => void, error?: (error: ResponseStatus) => void, complete?: () => void) {
        if (ok)
            this._success = ok;

        if (error)
            this._error = error;

        if (complete)
            this._complete = complete;

        this.IsRunning = true;

        this._task.finally(() => this.handleComplete()).subscribe(v => this.handleSuccess(v), e => this.handleError(e));

        return this;
    }

    public get ErrorMessage(): string {
        if (this.Error)
            return this.Error.Message;
        else
            "";
    }

    private handleSuccess(value: T) {
        this.Result = value;

        this._success(value);
    }

    private handleError(error: ResponseStatus) {
        this.Error = error;
        this.IsFaulted = true;

        this._error(error);
    }

    private handleComplete() {
        this.IsRunning = false;
        this.IsCompleted = true;

        this._complete();
    }
}

export interface IDictionary<T> {
    Add(key: string, value: T);
    ContainsKey(key: string): boolean;
    Count(): number;
    Item(key: string): T;
    Keys(): string[];
    Remove(key: string): T;
    Values(): T[];
}

export class Dictionary<T> implements IDictionary<T> {
    private items: { [key: string]: T } = {};

    private count: number = 0;

    public ContainsKey(key: string): boolean {
        return this.items.hasOwnProperty(key);
    }

    public Count(): number {
        return this.count;
    }

    public Add(key: string, value: T) {
        this.items[key] = value;
        this.count++;
    }

    public Remove(key: string): T {
        var val = this.items[key];
        delete this.items[key];
        this.count--;
        return val;
    }

    public Item(key: string): T {
        return this.items[key];
    }

    public Keys(): string[] {
        var keySet: string[] = [];

        for (var prop in this.items) {
            if (this.items.hasOwnProperty(prop)) {
                keySet.push(prop);
            }
        }

        return keySet;
    }

    public Values(): T[] {
        var values: T[] = [];

        for (var prop in this.items) {
            if (this.items.hasOwnProperty(prop)) {
                values.push(this.items[prop]);
            }
        }

        return values;
    }
}

export interface Serializable<T> {
    Deserialize(input: any): T;
}

export class Guid {
    static New(): string {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }
}

export class WebUtility {
    public static EncodeURIComponent(str: string): string {
        return encodeURIComponent(str).replace(/[!'(.)*]/g, function (c) {
            return '%' + c.charCodeAt(0).toString(16);
        });
    }
}