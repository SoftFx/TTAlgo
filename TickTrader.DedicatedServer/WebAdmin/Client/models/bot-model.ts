import { IDictionary } from './core';
import { Guid } from './guid';

export class BotModel {
    public readonly name: string;
    public readonly setup: BotSetup;

    constructor(name: string, setup: BotSetup) {
        this.name = name;
        this.setup = setup;
    }
}

export class ExtBotModel extends BotModel {
    private _state: BotState;

    public instanceId: string;
    public account: string;
    public symbol: string;

    constructor(name: string, setup: BotSetup, instanceId?: string, active?: boolean) {
        super(name, setup);
        this.instanceId = !instanceId ? name + ' (' + Guid.New()+')' : instanceId;
        this.state = active ? BotState.Runned : BotState.Stopped;
        this.account = "";
        this.symbol = "";
    }

    get state(): BotState {
        return this._state;
    }

    set state(value: BotState) {
        this._state = value;
    }

    get isActive(): boolean {
        return (this.state == BotState.Runned || this.state == BotState.Stopping);
    }
}

export class BotSetup {
    public parameters: Parameter[];

    constructor(...parametrs: Parameter[]) {
        this.parameters = parametrs;
    }

    public reset(): void {
        this.parameters.forEach(x => x.reset());
    }
}

export class Parameter {
    private readonly defaultValue: Object;

    public readonly displayName: string;
    public readonly enumValues: IDictionary<Object>;
    public readonly required: boolean;
    public readonly name: string;
    public readonly type: ParameterType;
    public value: Object;

    constructor(name: string, type: ParameterType, displayName?: string, defaultValue?: Object, enumValues?: IDictionary<Object>) {
        if (type == ParameterType.Enum) {
            if (!enumValues) throw new Error("ArgumentNullException: parameter #enumValue# is not specified");
        }
        this.name = name;
        this.type = type;
        this.displayName = displayName ? displayName : name;
        this.defaultValue = defaultValue;
        this.value = defaultValue;
        this.enumValues = enumValues;

        this.required = true;
    }

    public reset(): void {
        this.value = this.defaultValue;
    }

    public static CreateNumberParameter(name: string, defaultValue?: number, displayName?: string) {
        return new Parameter(name, ParameterType.Number, displayName, defaultValue);
    }
    public static CreateStringParameter(name: string, defaultValue?: string, displayName?: string) {
        return new Parameter(name, ParameterType.String, displayName, defaultValue);
    }
    public static CreateEnumParametr(name: string, enumValues: IDictionary<Object>, defaultValue?: Object, displayName?: string, ) {
        return new Parameter(name, ParameterType.Enum, displayName, defaultValue, enumValues);
    }
}

export enum ParameterType {
    Enum = 0,
    Number,
    String
}

export enum BotState {
    Runned = 0,
    Running,
    Stopping,
    Stopped
}