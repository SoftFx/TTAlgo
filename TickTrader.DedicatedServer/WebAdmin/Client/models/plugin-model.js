"use strict";
var PluginModel = (function () {
    function PluginModel(packageName) {
        this.Package = packageName;
    }
    Object.defineProperty(PluginModel.prototype, "IsIndicator", {
        get: function () {
            return this.Type.toLowerCase() == "indicator";
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(PluginModel.prototype, "IsRobot", {
        get: function () {
            return this.Type.toLowerCase() == "robot";
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(PluginModel.prototype, "Icon", {
        get: function () {
            if (this.IsIndicator) {
                return '&Iota;';
            }
            else {
                return '&Beta;';
            }
        },
        enumerable: true,
        configurable: true
    });
    PluginModel.prototype.Deserialize = function (input) {
        this.Id = input.Id;
        this.DisplayName = input.DisplayName;
        this.Type = input.Type;
        this.Parameters = input.Parameters.map(function (p) { return new ParameterDescriptor().Deserialize(p); });
        return this;
    };
    return PluginModel;
}());
exports.PluginModel = PluginModel;
var ParameterDescriptor = (function () {
    function ParameterDescriptor() {
    }
    ParameterDescriptor.prototype.Deserialize = function (input) {
        this.Id = input.Id;
        this.DisplayName = input.DisplayName;
        this.DataType = ParameterDataTypes[input.DataType];
        this.DefaultValue = input.DefaultValue;
        this.EnumValues = input.EnumValues;
        this.FileFilter = input.FileFilter;
        this.IsEnum = input.IsEnum;
        this.IsRequired = input.IsRequired;
        return this;
    };
    return ParameterDescriptor;
}());
exports.ParameterDescriptor = ParameterDescriptor;
var ParameterDataTypes;
(function (ParameterDataTypes) {
    ParameterDataTypes[ParameterDataTypes["Unknown"] = -1] = "Unknown";
    ParameterDataTypes[ParameterDataTypes["Int"] = 0] = "Int";
    ParameterDataTypes[ParameterDataTypes["Double"] = 1] = "Double";
    ParameterDataTypes[ParameterDataTypes["String"] = 2] = "String";
    ParameterDataTypes[ParameterDataTypes["File"] = 3] = "File";
    ParameterDataTypes[ParameterDataTypes["Enum"] = 4] = "Enum";
})(ParameterDataTypes = exports.ParameterDataTypes || (exports.ParameterDataTypes = {}));
//# sourceMappingURL=plugin-model.js.map