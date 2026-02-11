(function () {
    // Plays a single sine tone with the given frequency, duration, and gain.
    function beepTone(frequency, durationMs, gainValue) {
        var AudioContextCtor = window.AudioContext || window.webkitAudioContext;
        if (!AudioContextCtor) {
            return;
        }

        var context = new AudioContextCtor();
        var oscillator = context.createOscillator();
        var gain = context.createGain();

        oscillator.type = "sine";
        oscillator.frequency.value = frequency;
        gain.gain.value = gainValue;

        oscillator.connect(gain);
        gain.connect(context.destination);

        oscillator.start();
        oscillator.stop(context.currentTime + (durationMs / 1000));

        oscillator.onended = function () {
            context.close();
        };
    }

    window.taskflowAudio = window.taskflowAudio || {};
    // Plays TaskFlow feedback sounds for timer events.
    window.taskflowAudio.beep = function (eventType) {
        if (eventType === "finish") {
            beepTone(988, 180, 0.08);
            setTimeout(function () { beepTone(1318, 220, 0.08); }, 190);
            return;
        }

        beepTone(740, 130, 0.06);
    };

    window.taskflowUi = window.taskflowUi || {};
    // Focuses an element by id and selects its text content when supported.
    window.taskflowUi.focusById = function (id) {
        if (!id) {
            return;
        }

        var element = document.getElementById(id);
        if (!element) {
            return;
        }

        element.focus();
        if (typeof element.select === "function") {
            element.select();
        }
    };

    // Installs a global Escape key handler and forwards close requests to Blazor.
    window.taskflowUi.registerEscapeHandler = function (dotNetReference) {
        if (!dotNetReference) {
            return;
        }

        if (window.taskflowUi._escapeHandler) {
            document.removeEventListener("keydown", window.taskflowUi._escapeHandler, true);
        }

        window.taskflowUi._escapeHandler = function (event) {
            if (event.key === "Escape") {
                var shortcutsBackdrop = document.getElementById("shortcuts-backdrop");
                if (shortcutsBackdrop) {
                    shortcutsBackdrop.click();
                    return;
                }

                dotNetReference.invokeMethodAsync("HandleGlobalEscapeAsync");
            }
        };

        document.addEventListener("keydown", window.taskflowUi._escapeHandler, true);
    };

    // Removes the global Escape key handler installed by registerEscapeHandler.
    window.taskflowUi.unregisterEscapeHandler = function () {
        if (!window.taskflowUi._escapeHandler) {
            return;
        }

        document.removeEventListener("keydown", window.taskflowUi._escapeHandler, true);
        window.taskflowUi._escapeHandler = null;
    };

    // Selects the currently focused input-like element, if selectable.
    window.taskflowUi.selectFocusedInput = function () {
        var active = document.activeElement;
        if (!active) {
            return;
        }

        if (typeof active.select === "function") {
            active.select();
        }
    };

    // Finds an inline editor input by data attribute, then focuses and selects it.
    window.taskflowUi.focusInlineEditById = function (inlineEditId) {
        if (!inlineEditId) {
            return;
        }

        var selector = '[data-inline-edit-id="' + inlineEditId + '"] input';
        var input = document.querySelector(selector);
        if (!input) {
            return;
        }

        if (typeof input.focus === "function") {
            input.focus();
        }

        if (typeof input.select === "function") {
            input.select();
        }
    };

    // Focuses and selects the first input inside a container element by id.
    window.taskflowUi.focusInputInContainerById = function (containerId) {
        if (!containerId) {
            return;
        }

        var container = document.getElementById(containerId);
        if (!container) {
            return;
        }

        var input = container.querySelector("input");
        if (!input) {
            return;
        }

        if (typeof input.focus === "function") {
            input.focus();
        }

        if (typeof input.select === "function") {
            input.select();
        }
    };

    // Focuses the first text input inside the currently open MudBlazor dialog.
    window.taskflowUi.focusFirstDialogInput = function () {
        var input = document.querySelector(".mud-dialog input");
        if (!input) {
            return;
        }

        if (typeof input.focus === "function") {
            input.focus();
        }

        if (typeof input.select === "function") {
            input.select();
        }
    };

    window.taskflowPwa = window.taskflowPwa || {};
    // Registers the service worker to enable installability and static asset caching.
    window.taskflowPwa.register = function () {
        if (!("serviceWorker" in navigator)) {
            return;
        }

        window.addEventListener("load", function () {
            navigator.serviceWorker.register("/sw.js").catch(function () {
                // Ignore registration failures to avoid breaking the app runtime.
            });
        });
    };

})();
