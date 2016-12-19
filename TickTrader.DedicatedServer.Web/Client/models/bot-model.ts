export class BotModel {
    constructor(public id: number, public name: string) { }
}

export class ExtBotModel extends BotModel {
    private _state: BotState;

    executionId: string;

    constructor(id: number, name: string, executionId: string, active: boolean = false) {
        super(id, name);
        this.executionId = executionId;
        this.state = active ? BotState.Runned : BotState.Stopped;
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

export enum BotState {
    Runned = 0,
    Running,
    Stopping,
    Stopped
}