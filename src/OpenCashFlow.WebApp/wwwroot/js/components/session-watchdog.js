(function () {
    "use strict";

    const script = document.currentScript;
    if (!script) {
        return;
    }

    const disconnectUrl = script.dataset.disconnectUrl || "/Account/Disconnect";
    const authInfoCookieName = script.dataset.authInfoCookieName;
    const inactivityMinutes = Number(script.dataset.inactivityMinutes || "0");
    const refreshUrl = script.dataset.refreshUrl;

    if (!authInfoCookieName || !inactivityMinutes || !refreshUrl) {
        return;
    }

    const checkIntervalMs = 30 * 1000;
    const inactivityMs = inactivityMinutes * 60 * 1000;
    const refreshThresholdSeconds = 480;
    const refreshCheckIntervalMs = 120 * 1000;
    const minRefreshIntervalMs = 90 * 1000;

    let expirationTimeoutId = null;
    let checkIntervalId = null;
    let inactivityTimeoutId = null;
    let refreshCheckIntervalId = null;
    let hadToken = false;
    let lastRefreshTime = 0;
    let lastActivityTime = Date.now();
    let isRefreshing = false;

    function getCookie(name) {
        const value = "; " + document.cookie;
        const parts = value.split("; " + name + "=");
        if (parts.length === 2) {
            const part = parts.pop();
            return part ? part.split(";").shift() : null;
        }

        return null;
    }

    function disconnect() {
        if (window.top && window.top !== window.self) {
            window.top.location.href = disconnectUrl;
        } else {
            window.location.replace(disconnectUrl);
        }
    }

    function checkTokenExpiry() {
        const expirationTimestamp = getCookie(authInfoCookieName);

        if (!expirationTimestamp) {
            if (hadToken) {
                disconnect();
            }

            return;
        }

        hadToken = true;
        const expirationTime = parseInt(expirationTimestamp, 10);
        const now = Math.floor(Date.now() / 1000);
        const secondsRemaining = expirationTime - now;

        if (secondsRemaining <= 0) {
            disconnect();
            return;
        }

        if (expirationTimeoutId !== null) {
            clearTimeout(expirationTimeoutId);
        }

        expirationTimeoutId = setTimeout(disconnect, secondsRemaining * 1000);
    }

    async function refreshTokenIfNeeded() {
        const now = Date.now();

        if (now - lastRefreshTime < minRefreshIntervalMs || isRefreshing) {
            return;
        }

        const expirationTimestamp = getCookie(authInfoCookieName);
        if (!expirationTimestamp) {
            return;
        }

        const expirationTime = parseInt(expirationTimestamp, 10);
        const currentTime = Math.floor(now / 1000);
        const secondsRemaining = expirationTime - currentTime;

        if (secondsRemaining <= refreshThresholdSeconds && secondsRemaining > 0) {
            if (now - lastActivityTime > refreshCheckIntervalMs) {
                return;
            }

            isRefreshing = true;
            lastRefreshTime = now;

            try {
                const response = await fetch(refreshUrl, {
                    method: "POST",
                    credentials: "include"
                });

                if (!response.ok) {
                    console.error("[RefreshToken] Refresh failed:", response.status);
                }
            } catch (error) {
                console.error("[RefreshToken] Error:", error);
            } finally {
                isRefreshing = false;
            }
        }
    }

    function recordActivity() {
        lastActivityTime = Date.now();
    }

    function resetInactivityTimer() {
        if (inactivityTimeoutId) {
            clearTimeout(inactivityTimeoutId);
        }

        recordActivity();
        inactivityTimeoutId = setTimeout(disconnect, inactivityMs);
    }

    function cleanup() {
        if (checkIntervalId) {
            clearInterval(checkIntervalId);
        }

        if (expirationTimeoutId) {
            clearTimeout(expirationTimeoutId);
        }

        if (refreshCheckIntervalId) {
            clearInterval(refreshCheckIntervalId);
        }

        if (inactivityTimeoutId) {
            clearTimeout(inactivityTimeoutId);
        }
    }

    checkTokenExpiry();
    checkIntervalId = setInterval(checkTokenExpiry, checkIntervalMs);

    ["click", "keydown", "touchstart", "mousemove", "scroll"].forEach(function (eventName) {
        document.addEventListener(eventName, resetInactivityTimer, true);
    });

    refreshCheckIntervalId = setInterval(refreshTokenIfNeeded, refreshCheckIntervalMs);
    resetInactivityTimer();
    window.addEventListener("beforeunload", cleanup);
})();
