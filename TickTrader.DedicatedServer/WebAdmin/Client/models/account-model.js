"use strict";
var AccountModel = (function () {
    function AccountModel() {
        this.Login = "";
        this.Server = "";
        this.Password = "";
    }
    AccountModel.prototype.Deserialize = function (input) {
        this.Login = input.Login;
        this.Server = input.Server;
        return this;
    };
    return AccountModel;
}());
exports.AccountModel = AccountModel;
//# sourceMappingURL=account-model.js.map