(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        const passwordInput = document.getElementById("Password");
        const toggle = document.getElementById("togglePassword");

        if (!passwordInput || !toggle) {
            return;
        }

        toggle.addEventListener("click", function (event) {
            event.preventDefault();
            passwordInput.type = passwordInput.type === "password" ? "text" : "password";
        });
    });
})();
