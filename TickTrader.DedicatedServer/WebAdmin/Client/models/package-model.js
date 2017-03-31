"use strict";
var index_1 = require("./index");
var PackageModel = (function () {
    function PackageModel() {
    }
    Object.defineProperty(PackageModel.prototype, "Icon", {
        get: function () {
            return "fa fa-archive";
        },
        enumerable: true,
        configurable: true
    });
    PackageModel.prototype.Deserialize = function (input) {
        this.Created = input.Created;
        this.IsValid = input.IsValid;
        this.Name = input.Name;
        this.Plugins = input.Plugins ? input.Plugins.map(function (p) { return new index_1.PluginModel(input.Name).Deserialize(p); }) : input.Plugins;
        return this;
    };
    return PackageModel;
}());
exports.PackageModel = PackageModel;
//# sourceMappingURL=package-model.js.map