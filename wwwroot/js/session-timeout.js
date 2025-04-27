(function () {
    let warningTimer, logoutTimer, countdownInterval;
    let sessionModal; // For Bootstrap 4

    const warningTimeout = 1000 * 60 * 18; // 18 minutes of inactivity for warning
    const logoutTimeout = 1000 * 60 * 20;  // 20 minutes of inactivity for logout

    // Function to check if the current page is the login page
    function isLoginPage() {
        return window.location.pathname === "/Identity/Account/Login"; // Adjust URL path as needed
    }

    function resetTimers() {
        if (isLoginPage()) {
            console.log("User is on the login page, skipping session timeout.");
            return; // Stop session timeout logic if on login page
        }

        console.log("Resetting timers...");
        clearTimeout(warningTimer);
        clearTimeout(logoutTimer);

        warningTimer = setTimeout(showSessionWarning, warningTimeout);
        logoutTimer = setTimeout(logoutUser, logoutTimeout);
    }

    function showSessionWarning() {
        const modalEl = $('#sessionWarningModal'); // jQuery reference
        const countdownEl = document.getElementById("sessionCountdown");

        if (modalEl.length > 0) {
            sessionModal = modalEl; // store modal for later use
            sessionModal.modal('show');

            const continueBtn = document.getElementById("continueSessionBtn");
            if (continueBtn) {
                console.log("continueBtn found");
                continueBtn.onclick = stayLoggedIn;
            }

            // Replace the anchor logout link with a button click event
            const logoutBtn = document.getElementById("logoutBtn");
            if (logoutBtn) {
                logoutBtn.onclick = logoutUser;
            }

            let remaining = 120;
            if (countdownEl) countdownEl.textContent = remaining;

            countdownInterval = setInterval(() => {
                remaining--;
                if (countdownEl) countdownEl.textContent = remaining;

                if (remaining <= 0) {
                    clearInterval(countdownInterval);
                }
            }, 1000);
        }
    }

    function logoutUser() {
        console.log("Force Logging out now!");
        // Clear any session timeout or warning checks
        clearTimeout(warningTimer);
        clearTimeout(logoutTimer);
        clearInterval(countdownInterval);

        localStorage.clear();
        sessionStorage.clear();
        window.location.href = "/Home/ForceLogout";
    }

    function stayLoggedIn() {
        console.log("Stay Logged In clicked");

        // Clear session timeout and countdown immediately
        clearTimeout(logoutTimer);
        clearInterval(countdownInterval);

        if (sessionModal) sessionModal.modal('hide'); // Bootstrap 4 way

        fetch("/Home/PingSession")
            .then(() => {
                console.log("Ping successful. Resetting timers.");
                resetTimers(); // Reset timers after a successful session ping
            })
            .catch((err) => {
                console.log("Ping failed. Logging out.");
                logoutUser();
            });
    }

    window.stayLoggedIn = stayLoggedIn;

    // Attach event listeners to restart session checks
    document.addEventListener("DOMContentLoaded", resetTimers);
    document.addEventListener("mousemove", resetTimers);
    document.addEventListener("keypress", resetTimers);
    document.addEventListener("scroll", resetTimers);
    document.addEventListener("click", resetTimers);
})();
