"use strict";
var ResponseStatus = (function () {
    function ResponseStatus(response) {
        if (response)
            this.parse(response);
    }
    Object.defineProperty(ResponseStatus.prototype, "Handled", {
        get: function () { return this.Code !== -1; },
        enumerable: true,
        configurable: true
    });
    ResponseStatus.prototype.parse = function (response) {
        try {
            var responseBody = response.json();
            this.Code = responseBody.Code;
            this.Message = responseBody.Message;
            this.Ok = response.ok;
            return;
        }
        catch (ex) { }
        this.Code = ResponseCode.None;
        this.Status = response.status;
        this.Ok = response.ok;
        this.Message = response.statusText;
    };
    return ResponseStatus;
}());
exports.ResponseStatus = ResponseStatus;
var ResponseCode;
(function (ResponseCode) {
    ResponseCode[ResponseCode["None"] = -1] = "None";
    ResponseCode[ResponseCode["DuplicatePackage"] = 1000] = "DuplicatePackage";
    ResponseCode[ResponseCode["DuplicateAccount"] = 2000] = "DuplicateAccount";
})(ResponseCode = exports.ResponseCode || (exports.ResponseCode = {}));
var ConnectionErrorCodes;
(function (ConnectionErrorCodes) {
    ConnectionErrorCodes[ConnectionErrorCodes["None"] = 0] = "None";
    ConnectionErrorCodes[ConnectionErrorCodes["Unknown"] = 1] = "Unknown";
    ConnectionErrorCodes[ConnectionErrorCodes["NetworkError"] = 2] = "NetworkError";
    ConnectionErrorCodes[ConnectionErrorCodes["Timeout"] = 3] = "Timeout";
    ConnectionErrorCodes[ConnectionErrorCodes["BlockedAccount"] = 4] = "BlockedAccount";
    ConnectionErrorCodes[ConnectionErrorCodes["ClientInitiated"] = 5] = "ClientInitiated";
    ConnectionErrorCodes[ConnectionErrorCodes["InvalidCredentials"] = 6] = "InvalidCredentials";
    ConnectionErrorCodes[ConnectionErrorCodes["SlowConnection"] = 7] = "SlowConnection";
    ConnectionErrorCodes[ConnectionErrorCodes["ServerError"] = 8] = "ServerError";
    ConnectionErrorCodes[ConnectionErrorCodes["LoginDeleted"] = 9] = "LoginDeleted";
    ConnectionErrorCodes[ConnectionErrorCodes["ServerLogout"] = 10] = "ServerLogout";
    ConnectionErrorCodes[ConnectionErrorCodes["Canceled"] = 11] = "Canceled";
})(ConnectionErrorCodes = exports.ConnectionErrorCodes || (exports.ConnectionErrorCodes = {}));
var ConnectionTestResult = (function () {
    function ConnectionTestResult(code) {
        this.ConnectionErrorCode = code;
    }
    Object.defineProperty(ConnectionTestResult.prototype, "Message", {
        get: function () {
            switch (this.ConnectionErrorCode) {
                case ConnectionErrorCodes.Unknown:
                case ConnectionErrorCodes.NetworkError:
                    return "Network Error";
                case ConnectionErrorCodes.Timeout:
                    return "Timeout";
                case ConnectionErrorCodes.BlockedAccount:
                    return "Blocked account";
                case ConnectionErrorCodes.InvalidCredentials:
                    return "Invalid credentials";
                case ConnectionErrorCodes.SlowConnection:
                    return "Slow connection";
                case ConnectionErrorCodes.ServerError:
                    return "Server error";
                case ConnectionErrorCodes.LoginDeleted:
                    return "Login deleted";
                case ConnectionErrorCodes.ServerLogout:
                    return "Server logout";
                case ConnectionErrorCodes.Canceled:
                    return "Canceled";
                default: return "Connection success";
            }
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(ConnectionTestResult.prototype, "TestPassed", {
        get: function () {
            switch (this.ConnectionErrorCode) {
                case ConnectionErrorCodes.Unknown:
                case ConnectionErrorCodes.NetworkError:
                case ConnectionErrorCodes.Timeout:
                case ConnectionErrorCodes.BlockedAccount:
                case ConnectionErrorCodes.InvalidCredentials:
                case ConnectionErrorCodes.SlowConnection:
                case ConnectionErrorCodes.ServerError:
                case ConnectionErrorCodes.LoginDeleted:
                case ConnectionErrorCodes.ServerLogout:
                case ConnectionErrorCodes.Canceled:
                    return false;
                default: return true;
            }
        },
        enumerable: true,
        configurable: true
    });
    return ConnectionTestResult;
}());
exports.ConnectionTestResult = ConnectionTestResult;
//# sourceMappingURL=response.js.map