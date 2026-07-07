(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        const form = document.getElementById("formAuthentication");
        const password = document.getElementById("Password");
        const confirmPassword = document.getElementById("ConfirmPassword");
        const strongPasswordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=[\]{};':"\\|,.<>/?]).{8,}$/;

        form?.addEventListener("submit", function (event) {
            const passwordValue = password?.value ?? "";
            const confirmPasswordValue = confirmPassword?.value ?? "";

            if (!strongPasswordPattern.test(passwordValue)) {
                event.preventDefault();
                alert("The password must be at least 8 characters, with one uppercase, one lowercase, one number, and one special character.");
                return;
            }

            if (passwordValue !== confirmPasswordValue) {
                event.preventDefault();
                alert("Passwords do not match.");
            }
        });
    });
})();
