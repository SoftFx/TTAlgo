import { Serializable, Guid } from './index';

export class AccountModel implements Serializable<AccountModel> {
    public Login: string = "";
    public Server: string = "";
    public Password: string = "";

    constructor() { }

    public Deserialize(input: any): AccountModel {
        this.Login = input.Login;
        this.Server = input.Server;

        return this;
    }
}

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
        this.Plugins = input.Plugins ? input.Plugins.map(p => new PluginModel(input.Name).Deserialize(p)) : input.Plugins;

        return this;
    }
}

export class PluginModel implements Serializable<PluginModel>{
    public Id: string;
    public DisplayName: string;
    public Type: string;
    public Parameters: ParameterDescriptor[];
    public Package: string;

    constructor(packageName?: string) {
        this.Package = packageName;
    }

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

    public Deserialize(input: any): PluginModel {
        this.Id = input.Id;
        this.DisplayName = input.DisplayName;
        this.Type = input.Type;
        this.Parameters = input.Parameters.map(p => new ParameterDescriptor().Deserialize(p));

        return this;
    }
}

export class TradeBotModel implements Serializable<TradeBotModel>{
    public Id: string;
    public Status: string;
    public Account: AccountModel;
    public State: TradeBotStates;
    public Config: TradeBotConfig;

    constructor() { }

    public Deserialize(input: any): TradeBotModel {
        this.Id = input.Id;
        this.Status = input["Status"] ? input.Status : "";
        this.Account = new AccountModel().Deserialize(input.Account);
        this.State = TradeBotStates[input.State as string];
        this.Config = new TradeBotConfig().Deserialize(input.Config);

        return this;
    }
}

export class TradeBotStateModel implements Serializable<TradeBotStateModel>{
    public Id: string;
    public State: TradeBotStates;

    constructor() { }


    public Deserialize(input: any): TradeBotStateModel {
        this.Id = input.Id;
        this.State = TradeBotStates[input.State as string];

        return this;
    }
}

export enum TradeBotStates { Offline, Starting, Started, Initializing, Faulted, Online, Stopping }

export class TradeBotConfig implements Serializable<TradeBotConfig>{
    public Symbol: string;
    public Parameters: Parameter[];

    public Deserialize(input: any): TradeBotConfig {
        this.Symbol = input["Symbol"] ? input.Symbol : "";
        this.Parameters = input.Parameters.map(p => new Parameter(p.Value, new ParameterDescriptor().Deserialize(p.Descriptor)));
        return this;
    }
}

export class Parameter {
    private _value: any;

    constructor(descriptor: ParameterDescriptor, value?: any) {
        this.Descriptor = descriptor;
        this.Value = value ? value : descriptor.DefaultValue;
    }

    public Descriptor: ParameterDescriptor;


    public get Value() {
        return this._value;
    }

    public set Value(value: any) {
        switch (this.Descriptor.DataType) {
            case ParameterDataTypes.Int:
                this._value = Math.floor(value);
                break;
            default:
                this._value = value;
        }
    }
}

export class ParameterDescriptor implements Serializable<ParameterDescriptor>{

    public Id: string;
    public DisplayName: string;
    public DataType: ParameterDataTypes;
    public DefaultValue: any;
    public EnumValues: string[];
    public FileFilter: string;
    public IsEnum: boolean;
    public IsRequired: boolean;

    public Deserialize(input): ParameterDescriptor {
        this.Id = input.Id;
        this.DisplayName = input.DisplayName;
        this.DataType = ParameterDataTypes[input.DataType as string];
        this.DefaultValue = input.DefaultValue;
        this.EnumValues = input.EnumValues;
        this.FileFilter = input.FileFilter;
        this.IsEnum = input.IsEnum;
        this.IsRequired = input.IsRequired;

        return this;
    }
}

export enum ParameterDataTypes {
    Unknown = -1,
    Int,
    Double,
    String,
    File,
    Enum
}

export class SetupModel {
    public PackageName: string;
    public PluginId: string;
    public InstanceId: string;
    public Account: AccountModel;
    public Symbol: string;

    public Parameters: Parameter[];


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
    private PayloadParameter(parameter: Parameter) {
        return { Id: parameter.Descriptor.Id, Value: parameter.Value, DataType: ParameterDataTypes[parameter.Descriptor.DataType] }
    }

    public static ForTradeBot(bot: TradeBotModel) {
        let setup = new SetupModel();
        setup.Symbol = bot.Config.Symbol;
        setup.InstanceId = bot.Id;
        setup.Parameters = bot.Config.Parameters.map(p => new Parameter(p.Value, p.Descriptor));
        setup.Account = bot.Account;

        return setup;
    }

    public static ForPlugin(plugin: PluginModel) {
        let setup = new SetupModel();
        setup.PackageName = plugin.Package;
        setup.PluginId = plugin.Id;
        setup.InstanceId = `${plugin.DisplayName} [${Guid.New().substring(0, 4)}]`;
        setup.Parameters = plugin.Parameters.map(p => new Parameter(p));
        setup.Account = null;
        setup.Symbol = "";

        return setup;
    }
}

