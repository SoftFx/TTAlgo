"use strict";
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var guid_1 = require("./guid");
var BotModel = (function () {
    function BotModel(name, setup) {
        this.name = name;
        this.setup = setup;
    }
    return BotModel;
}());
exports.BotModel = BotModel;
var ExtBotModel = (function (_super) {
    __extends(ExtBotModel, _super);
    function ExtBotModel(name, setup, instanceId, active) {
        var _this = _super.call(this, name, setup) || this;
        _this.instanceId = !instanceId ? name + ' (' + guid_1.Guid.New() + ')' : instanceId;
        _this.state = active ? BotState.Runned : BotState.Stopped;
        _this.account = "";
        _this.symbol = "";
        return _this;
    }
    Object.defineProperty(ExtBotModel.prototype, "state", {
        get: function () {
            return this._state;
        },
        set: function (value) {
            this._state = value;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(ExtBotModel.prototype, "isActive", {
        get: function () {
            return (this.state == BotState.Runned || this.state == BotState.Stopping);
        },
        enumerable: true,
        configurable: true
    });
    return ExtBotModel;
}(BotModel));
exports.ExtBotModel = ExtBotModel;
var BotSetup = (function () {
    function BotSetup() {
        var parametrs = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            parametrs[_i] = arguments[_i];
        }
        this.parameters = parametrs;
    }
    BotSetup.prototype.reset = function () {
        this.parameters.forEach(function (x) { return x.reset(); });
    };
    return BotSetup;
}());
exports.BotSetup = BotSetup;
var Parameter = (function () {
    function Parameter(name, type, displayName, defaultValue, enumValues) {
        if (type == ParameterType.Enum) {
            if (!enumValues)
                throw new Error("ArgumentNullException: parameter #enumValue# is not specified");
        }
        this.name = name;
        this.type = type;
        this.displayName = displayName ? displayName : name;
        this.defaultValue = defaultValue;
        this.value = defaultValue;
        this.enumValues = enumValues;
        this.required = true;
    }
    Parameter.prototype.reset = function () {
        this.value = this.defaultValue;
    };
    Parameter.CreateNumberParameter = function (name, defaultValue, displayName) {
        return new Parameter(name, ParameterType.Number, displayName, defaultValue);
    };
    Parameter.CreateStringParameter = function (name, defaultValue, displayName) {
        return new Parameter(name, ParameterType.String, displayName, defaultValue);
    };
    Parameter.CreateEnumParametr = function (name, enumValues, defaultValue, displayName) {
        return new Parameter(name, ParameterType.Enum, displayName, defaultValue, enumValues);
    };
    return Parameter;
}());
exports.Parameter = Parameter;
var ParameterType;
(function (ParameterType) {
    ParameterType[ParameterType["Enum"] = 0] = "Enum";
    ParameterType[ParameterType["Number"] = 1] = "Number";
    ParameterType[ParameterType["String"] = 2] = "String";
})(ParameterType = exports.ParameterType || (exports.ParameterType = {}));
var BotState;
(function (BotState) {
    BotState[BotState["Runned"] = 0] = "Runned";
    BotState[BotState["Running"] = 1] = "Running";
    BotState[BotState["Stopping"] = 2] = "Stopping";
    BotState[BotState["Stopped"] = 3] = "Stopped";
})(BotState = exports.BotState || (exports.BotState = {}));
//# sourceMappingURL=bot-model.js.map