import { Serializable, PluginModel } from './index';

export class PackageModel implements Serializable<PackageModel> {
    public Name: string;
    public Created: Date;
    public IsValid: boolean;
    public Plugins: PluginModel[];

    public get Icon(): string {
        return "fa fa-archive";
    }

    public Deserialize(input: any): PackageModel {
        this.Created = input.Created;
        this.IsValid = input.IsValid;
        this.Name = input.Name;
        this.Plugins = input.Plugins ? input.Plugins.map(p => new PluginModel().Deserialize(p)) : input.Plugins;

        return this;
    }
}