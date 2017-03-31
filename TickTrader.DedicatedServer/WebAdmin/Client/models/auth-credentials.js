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
//# sourceMappingURL=auth-credentials.js.map