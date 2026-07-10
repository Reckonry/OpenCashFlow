(function () {
    "use strict";

    function getToken() {
        const meta = document.querySelector('meta[name="request-verification-token"]');
        if (meta && meta.content) {
            return meta.content;
        }

        const input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : "";
    }

    window.openCashFlowAntiForgeryToken = getToken;
    window.openCashFlowAntiForgeryHeaders = function () {
        const token = getToken();
        return token ? { "RequestVerificationToken": token } : {};
    };
})();
