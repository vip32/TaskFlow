(function () {
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
    window.taskflowAudio.beep = function (eventType) {
        if (eventType === "finish") {
            beepTone(988, 180, 0.08);
            setTimeout(function () { beepTone(1318, 220, 0.08); }, 190);
            return;
        }

        beepTone(740, 130, 0.06);
    };

    window.taskflowUi = window.taskflowUi || {};
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

    window.taskflowUi.unregisterEscapeHandler = function () {
        if (!window.taskflowUi._escapeHandler) {
            return;
        }

        document.removeEventListener("keydown", window.taskflowUi._escapeHandler, true);
        window.taskflowUi._escapeHandler = null;
    };

})();
