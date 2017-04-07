"use strict";
var AuthCredentials = (function () {
    function AuthCredentials(login, password) {
        if (login === void 0) { login = ""; }
        if (password === void 0) { password = ""; }
        this.login = login;
        this.password = password;
    }
    return AuthCredentials;
}());
exports.AuthCredentials = AuthCredentials;
var AuthData = (function () {
    function AuthData(token, expires, user) {
        this.token = token;
        this.expires = new Date(expires);
        this.user = user;
    }
    AuthData.prototype.IsEmpty = function () {
        return !this.token;
    };
    return AuthData;
}());
exports.AuthData = AuthData;
//# sourceMappingURL=auth-credentials.js.map