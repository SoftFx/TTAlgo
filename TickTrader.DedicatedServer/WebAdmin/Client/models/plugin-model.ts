import { Serializable } from './index';

export class PluginModel implements Serializable<PluginModel>{
    public id: string;
    public displayName: string;
    public type: string;

    public get isIndicator() {
        return this.type.toLowerCase() == "indicator"
    }

    public get isRobot() {
        return this.type.toLowerCase() == "robot"
    }

    public get icon(): string {
        if (this.isIndicator) {
            return '&Iota;';
        }
        else {
            return '&Beta;';
        }
    }

    public deserialize(input): PluginModel {
        return Object.assign(new PluginModel(), input);
    }
}