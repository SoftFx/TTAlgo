"use strict";
var Utils = (function () {
    function Utils() {
    }
    // Convert date string to Date instance
    Utils.DateReviver = function (key, value) {
        var a;
        if (typeof value === 'string') {
            a = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2}(?:\.\d*)?)Z$/.exec(value);
            if (a) {
                return new Date(Date.UTC(+a[1], +a[2] - 1, +a[3], +a[4], +a[5], +a[6]));
            }
        }
        return value;
    };
    ;
    return Utils;
}());
exports.Utils = Utils;
//# sourceMappingURL=utils.js.map