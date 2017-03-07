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
        let packageModel = new PackageModel();
        packageModel.Created = input.Created;
        packageModel.IsValid = input.IsValid;
        packageModel.Name = input.Name;
        packageModel.Plugins = input.Plugins ? input.Plugins.map(p => new PluginModel().Deserialize(p)) : input.Plugins;

        return packageModel;
    }
}