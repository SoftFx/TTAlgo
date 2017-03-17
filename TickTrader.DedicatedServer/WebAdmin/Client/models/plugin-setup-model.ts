import { AccountModel } from './account-model'
import { Guid } from './guid';
import { PluginModel, ParameterDescriptor, ParameterDataTypes } from './plugin-model';


export class PluginSetupModel {
    public PackageName: string;
    public PluginId: string;
    public InstanceId: string;
    public Account: AccountModel;
    public Symbol: string;

    public Parameters: SetupParameter[];


    public get Payload() {

        return {
            PackageName: this.PackageName,
            PluginId: this.PluginId,
            InstanceId: this.InstanceId,
            Account: this.Account,
            Symbol: this.Symbol,
            Parameters: this.Parameters.map(p => this.PayloadParameter(p))


        }
    }
    private PayloadParameter(parameter: SetupParameter) {

        if (parameter.Descriptor.DataType === ParameterDataTypes.File) {
            let input = new FormData();
            input.append("file", parameter.Value);
            return { Id: parameter.Descriptor.Id, Value: input, DataType: ParameterDataTypes[parameter.Descriptor.DataType] }
        }
        else {
            return { Id: parameter.Descriptor.Id, Value: parameter.Value, DataType: ParameterDataTypes[parameter.Descriptor.DataType] }
        }
    }

    public static Create(plugin: PluginModel) {
        let setup = new PluginSetupModel();
        setup.PackageName = plugin.Package;
        setup.PluginId = plugin.Id;
        setup.InstanceId = `${plugin.DisplayName} [${Guid.New().substring(0, 4)}]`;
        setup.Parameters = plugin.Parameters.map(p => new SetupParameter(p));
        setup.Account = null;
        setup.Symbol = "";

        return setup;
    }
}

export class SetupParameter {
    constructor(descriptor: ParameterDescriptor) {
        this.Descriptor = descriptor;
    }

    public Descriptor: ParameterDescriptor;
    public Value: any;
    public OnValueChanged(value: any) {
        this.Value = value.target.files[0];
    }
    public get FileName() {
        if (this.Value)
            return this.Value.name;
        return "";
    }
}