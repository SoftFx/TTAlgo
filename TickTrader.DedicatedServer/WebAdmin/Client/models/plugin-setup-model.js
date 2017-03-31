"use strict";
var guid_1 = require("./guid");
var plugin_model_1 = require("./plugin-model");
var PluginSetupModel = (function () {
    function PluginSetupModel() {
    }
    Object.defineProperty(PluginSetupModel.prototype, "Payload", {
        get: function () {
            var _this = this;
            return {
                PackageName: this.PackageName,
                PluginId: this.PluginId,
                InstanceId: this.InstanceId,
                Account: this.Account,
                Symbol: this.Symbol,
                Parameters: this.Parameters.map(function (p) { return _this.PayloadParameter(p); })
            };
        },
        enumerable: true,
        configurable: true
    });
    PluginSetupModel.prototype.PayloadParameter = function (parameter) {
        return { Id: parameter.Descriptor.Id, Value: parameter.Value, DataType: plugin_model_1.ParameterDataTypes[parameter.Descriptor.DataType] };
    };
    PluginSetupModel.Create = function (plugin) {
        var setup = new PluginSetupModel();
        setup.PackageName = plugin.Package;
        setup.PluginId = plugin.Id;
        setup.InstanceId = plugin.DisplayName + " [" + guid_1.Guid.New().substring(0, 4) + "]";
        setup.Parameters = plugin.Parameters.map(function (p) { return new SetupParameter(p); });
        setup.Account = null;
        setup.Symbol = "";
        return setup;
    };
    return PluginSetupModel;
}());
exports.PluginSetupModel = PluginSetupModel;
var SetupParameter = (function () {
    function SetupParameter(descriptor) {
        this.Descriptor = descriptor;
    }
    return SetupParameter;
}());
exports.SetupParameter = SetupParameter;
//# sourceMappingURL=plugin-setup-model.js.map