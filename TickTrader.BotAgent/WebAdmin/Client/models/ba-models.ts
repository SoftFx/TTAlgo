import { Serializable, Guid } from './index';

export class AccountInfo implements Serializable<AccountInfo>{
    public Symbols: string[];

    public Deserialize(input: any): AccountInfo {
        this.Symbols = input.Symbols;

        return this;
    }

    public get MainSymbol(): string {
        if (this.Symbols) {
            let main = this.Symbols.find(s => s === "EURUSD" || s === "EUR/USD");
            return main ? main : this.Symbols[0];
        }
        else
            return "";
    }
}

export class AccountModel implements Serializable<AccountModel> {
    public Login: string = "";
    public Server: string = "";
    public Password: string = "";
    public UseNewProtocol: boolean = false;

    constructor() { }

    public Deserialize(input: any): AccountModel {
        this.Login = input.Login;
        this.Server = input.Server;
        this.UseNewProtocol = input.UseNewProtocol;

        return this;
    }

    public toString = (): string => {
        return `${this.Login} - ${this.Server}`;
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
    public UserDisplayName: string;
    public Type: string;
    public ParamDescriptors: ParameterDescriptor[];
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
        this.UserDisplayName = input.UserDisplayName;
        this.Type = input.Type;
        this.ParamDescriptors = input.Parameters.map(p => new ParameterDescriptor().Deserialize(p));

        return this;
    }
}

export enum LogEntryTypes { Info, Trading, Error, Custom, TradingSuccess, TradingFail }

export class LogEntry implements Serializable<LogEntry> {
    public Time: Date;
    public Type: LogEntryTypes;
    public Message: string;

    public Deserialize(input: any): LogEntry {
        this.Message = input.Message;
        this.Type = LogEntryTypes[input.Type as string];
        this.Time = new Date((input.Time as Date).toLocaleString());

        return this;
    }
}

export class TradeBotStatus implements Serializable<TradeBotStatus> {
    public Status: string;
    public BotId: string;

    public Deserialize(input: any): TradeBotStatus {
        this.Status = input.Status;
        this.BotId = input.BotId;

        return this;
    }
}

export class FileInfo implements Serializable<FileInfo> {
    public Name: string;
    public Size: number;

    public Deserialize(input: any): FileInfo {
        this.Name = input.Name;
        this.Size = input.Size;

        return this;
    }

    public get FormattedSize(): string {
        return this.calcSize();
    }

    public toString = (): string => {
        return `${this.Name} (${this.calcSize()})`;
    }

    private calcSize(): string {
        if (this.Size < 1024)
            return `${this.Size} bytes`;
        else if (this.Size < 1048576)
            return `${Math.floor(this.Size / 1024 * 10) / 10} KB`;
        else if (this.Size < 1073741824)
            return `${Math.floor(this.Size / 1024 / 1024 * 10) / 10} MB`;
        else
            return `${Math.floor(this.Size / 1024 / 1024 / 1024 * 10) / 10} GB`;
    }
}

export class TradeBotLog implements Serializable<TradeBotLog> {
    public Snapshot: LogEntry[];
    public Files: FileInfo[];

    public Deserialize(input: any): TradeBotLog {
        this.Snapshot = input.Snapshot.map(le => new LogEntry().Deserialize(le));
        this.Files = input.Files.map(f => new FileInfo().Deserialize(f));

        return this;
    }
}

export class TradeBotModel implements Serializable<TradeBotModel>{
    public Id: string;
    public Account: AccountModel;
    public State: TradeBotStates;
    public PackageName: string;
    public BotName: string;
    public FaultMessage: string;
    public Config: TradeBotConfig;
    public Permissions: TradeBotPermissions;

    constructor() { }

    public Deserialize(input: any): TradeBotModel {
        this.Id = input.Id;
        this.Account = new AccountModel().Deserialize(input.Account);
        this.State = TradeBotStates[input.State as string];
        this.PackageName = input.PackageName;
        this.BotName = input.BotName;
        this.FaultMessage = input.FaultMessage;
        this.Config = new TradeBotConfig().Deserialize(input.Config);
        this.Permissions = <TradeBotPermissions>input.Permissions;

        return this;
    }

    public get IsOnline(): boolean {
        return this.State === TradeBotStates.Running;
    }

    public get IsProcessing(): boolean {
        return this.State === TradeBotStates.Starting
            || this.State === TradeBotStates.Reconnecting
            || this.State === TradeBotStates.Stopping;
    }

    public get IsOffline(): boolean {
        return this.State === TradeBotStates.Stopped;
    }

    public get Faulted(): boolean {
        return this.State === TradeBotStates.Faulted;
    }

    public get CanStop(): boolean {
        return this.State === TradeBotStates.Running
            || this.State === TradeBotStates.Starting
            || this.State === TradeBotStates.Reconnecting;
    }

    public get CanStart(): boolean {
        return this.State === TradeBotStates.Stopped
            || this.State === TradeBotStates.Faulted;
    }

    public get CanDelete(): boolean {
        return this.State === TradeBotStates.Stopped
            || this.State === TradeBotStates.Faulted;
    }

    public get CanConfigurate(): boolean {
        return this.State === TradeBotStates.Stopped
            || this.State === TradeBotStates.Faulted;
    }
}

export class TradeBotStateModel implements Serializable<TradeBotStateModel>{
    public Id: string;
    public State: TradeBotStates;
    public FaultMessage: string;

    constructor() { }


    public Deserialize(input: any): TradeBotStateModel {
        this.Id = input.Id;
        this.State = TradeBotStates[input.State as string];
        this.FaultMessage = input.FaultMessage;

        return this;
    }

    public toString = (): string => {
        return `Bot '${this.Id}' ${TradeBotStates[this.State]} ${this.FaultMessage}`;
    }
}

export enum TradeBotStates { Stopped, Starting, Faulted, Running, Stopping, Broken, Reconnecting }

export class TradeBotConfig implements Serializable<TradeBotConfig>{
    public Symbol: string;
    public Parameters: Parameter[];

    public Deserialize(input: any): TradeBotConfig {
        this.Symbol = input["Symbol"] ? input.Symbol : "";
        this.Parameters = input.Parameters.map(p => new Parameter(p.Id, p.Value, p.Descriptor ? new ParameterDescriptor().Deserialize(p.Descriptor) : null));
        return this;
    }
}

export class Parameter {
    private _value: any;

    public Id: string;
    public Descriptor: ParameterDescriptor;

    constructor(id: string, value: any, descriptor: ParameterDescriptor) {
        this.Descriptor = descriptor;
        this.Id = id;
        this.Value = value;
    }

    public get Value() {
        return this._value;
    }

    public set Value(value: any) {
        if (!this.Descriptor)
            this._value = value;
        else {
            switch (this.Descriptor.DataType) {
                case ParameterDataTypes.Int:
                    this._value = Math.floor(value);
                    break;
                case ParameterDataTypes.Double:
                    this._value = value ? value : 0.0;
                    break;
                default:
                    this._value = value;
            }
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
    NInt,
    Double,
    NDouble,
    String,
    Boolean,
    File,
    Enum
}

export class SetupModel {
    public PackageName: string;
    public PluginId: string;
    public InstanceId: string;
    public Account: AccountModel;
    public Symbol: string;
    public Permissions: TradeBotPermissions;

    public Parameters: Parameter[];


    public get Payload() {

        return {
            PackageName: this.PackageName,
            PluginId: this.PluginId,
            InstanceId: this.InstanceId,
            Account: this.Account,
            Symbol: this.Symbol,
            Parameters: this.Parameters.map(p => this.PayloadParameter(p)),
            Permissions: this.Permissions
        }
    }
    private PayloadParameter(parameter: Parameter) {
        return { Id: parameter.Descriptor.Id, Value: parameter.Value, DataType: ParameterDataTypes[parameter.Descriptor.DataType] }
    }

    public static ForTradeBot(bot: TradeBotModel) {
        let setup = new SetupModel();
        setup.Symbol = bot.Config.Symbol;
        setup.InstanceId = bot.Id;
        setup.Parameters = bot.Config.Parameters.map(p => new Parameter(p.Id,
            p.Descriptor.DataType === ParameterDataTypes.File ? { FileName: p.Value, Size: 0, Data: null } : p.Value,
            p.Descriptor));
        setup.Account = bot.Account;
        setup.Permissions = <TradeBotPermissions>bot.Permissions;

        return setup;
    }

    public static ForPlugin(plugin: PluginModel) {
        let setup = new SetupModel();
        setup.PackageName = plugin.Package;
        setup.PluginId = plugin.Id;
        setup.Parameters = plugin.ParamDescriptors.map(pDescriptor =>
            new Parameter(pDescriptor.Id,
                pDescriptor.DataType === ParameterDataTypes.File ? { FileName: pDescriptor.DefaultValue, Size: 0, Data: null } : pDescriptor.DefaultValue,
                pDescriptor));
        setup.Account = null;
        setup.Symbol = "";

        let permissions = new TradeBotPermissions();
        permissions.TradeAllowed = true;
        permissions.Isolated = true;

        setup.Permissions = permissions;

        return setup;
    }
}

export class TradeBotPermissions
{
    public TradeAllowed: boolean;
    public Isolated: boolean;
}

