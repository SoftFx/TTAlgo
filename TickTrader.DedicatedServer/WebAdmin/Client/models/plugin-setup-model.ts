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
        return { Id: parameter.Descriptor.Id, Value: parameter.Value, DataType: ParameterDataTypes[parameter.Descriptor.DataType] }
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
    private _value: any;

    constructor(descriptor: ParameterDescriptor) {
        this.Descriptor = descriptor;
    }

    public Descriptor: ParameterDescriptor;
    

    public get Value() {
        return this._value;
    }

    public set Value(value: any) {
        switch (this.Descriptor.DataType)
        {
            case ParameterDataTypes.Int:
                this._value = Math.floor(value);
                break;
            default:
                this._value = value;
        }
    }
}