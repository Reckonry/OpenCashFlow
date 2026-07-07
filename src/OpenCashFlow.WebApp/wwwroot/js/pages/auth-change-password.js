(function () {
    "use strict";

    function isStrongPassword(password) {
        if (!password || password.length < 8) {
            return false;
        }

        return /[A-Z]/.test(password)
            && /[a-z]/.test(password)
            && /\d/.test(password)
            && /[^a-zA-Z0-9]/.test(password);
    }

    function updateRequirement(selector, isValid) {
        const element = $(selector);
        const icon = element.find("i");

        if (isValid) {
            element.removeClass("text-muted").addClass("text-success");
            icon.removeClass("ri-checkbox-blank-circle-line").addClass("ri-checkbox-circle-fill");
            return;
        }

        element.removeClass("text-success").addClass("text-muted");
        icon.removeClass("ri-checkbox-circle-fill").addClass("ri-checkbox-blank-circle-line");
    }

    $(function () {
        const form = $('form[action*="ChangePassword"]');
        const newPassword = $('input[name="NewPassword"]');
        const repeatPassword = $('input[name="RepeatPassword"]');
        const errorAlert = $("#passwordError");

        newPassword.on("input", function () {
            const password = $(this).val();

            updateRequirement("#req-length", password.length >= 8);
            updateRequirement("#req-uppercase", /[A-Z]/.test(password));
            updateRequirement("#req-lowercase", /[a-z]/.test(password));
            updateRequirement("#req-digit", /\d/.test(password));
            updateRequirement("#req-special", /[^a-zA-Z0-9]/.test(password));
        });

        $(".toggle-password").on("click", function () {
            const target = $(this).data("target");
            const input = $(target);
            const type = input.attr("type") === "password" ? "text" : "password";

            input.attr("type", type);
            $(this).find("i").toggleClass("ri-eye-off-line ri-eye-line");
        });

        form.on("submit", function (event) {
            const password = newPassword.val();
            const repeat = repeatPassword.val();

            errorAlert.addClass("d-none").text("");

            if (password !== repeat) {
                event.preventDefault();
                errorAlert.removeClass("d-none").text(errorAlert.data("passwordsMismatchMessage"));
                return false;
            }

            if (!isStrongPassword(password)) {
                event.preventDefault();
                errorAlert.removeClass("d-none").text(errorAlert.data("passwordRequirementsMessage"));
                return false;
            }
        });
    });
})();
