import { Serializable, PluginModel } from './index';

export class PackageModel implements Serializable<PackageModel> {
    public name: string;
    public created: Date;
    public isValid: boolean;
    public plugins: PluginModel[];

    public get icon(): string {
        return "fa fa-archive";
    }

    public deserialize(input: any): PackageModel {
        let packageModel = new PackageModel();
        packageModel.created = input.created;
        packageModel.isValid = input.isValid;
        packageModel.name = input.name;
        packageModel.plugins = input.plugins ? input.plugins.map(p => new PluginModel().deserialize(p)) : input.plugins;

        return packageModel;
    }
}