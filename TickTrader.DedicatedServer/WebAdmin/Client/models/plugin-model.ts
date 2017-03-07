import { Serializable } from './index';

export class PluginModel implements Serializable<PluginModel>{
    public Id: string;
    public DisplayName: string;
    public Type: string;

    public get IsIndicator() {
        return this.Type.toLowerCase() == "indicator"
    }

    public get IsRobot() {
        return this.Type.toLowerCase() == "robot"
    }

    public get Icon(): string {
        if (this.IsIndicator) {
            return '&Iota;';
        }
        else {
            return '&Beta;';
        }
    }

    public Deserialize(input): PluginModel {
        return Object.assign(new PluginModel(), input);
    }
}