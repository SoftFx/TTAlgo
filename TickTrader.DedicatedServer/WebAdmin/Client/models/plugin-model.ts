import { Serializable } from './index';

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