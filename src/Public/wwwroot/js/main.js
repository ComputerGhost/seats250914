/**
 * Helper functions
 */

function periodToSeconds(period) {
    const matches = period.match(/PT?(?:(\d+)M)?(?:(\d+)S)/);
    const minutes = matches[1] ? parseInt(matches[1]) : 0;
    const seconds = matches[2] ? parseInt(matches[2]) : 0;
    return minutes * 60 + seconds;
}

function secondsToPeriod(totalSeconds) {
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;
    return `PT${minutes}M${seconds}S`;
}

function setCookie(key, value, expires) {
    const encodedKey = encodeURIComponent(key);
    const encodedValue = encodeURIComponent(value);
    const formattedExpires = new Date(expires).toUTCString();
    document.cookie = `${encodedKey}=${encodedValue};expires=${formattedExpires};path=/`;
}

/**
 * Countdown timer.
 *
 * This converts a static time display into a countdown timer.
 *
 * To use, simply add the `countdown` CSS class to a `<time>` element that has
 * a period for its `datetime` attribute, formatted according to HTML5 specs.
 *
 * NOTE: Periods longer than PT59M59S are not supported.
 */
function Countdown($element) {
    const that = this;
    this.startTime = new Date();
    this.duration = periodToSeconds($element.attr("datetime"));
    this.timerId = setInterval(() => that.updateCountdown(), 500);

    this.updateCountdown = function () {
        const elapsed = (new Date() - that.startTime) / 1000;
        var remaining = that.duration - elapsed;

        if (remaining <= 0) {
            clearInterval(that.timerId);
            remaining = 0;
        }

        $element.attr("datetime", secondsToPeriod(remaining));
        $element.text(secondsToDisplay(remaining));
    };

    function secondsToDisplay(totalSeconds) {
        const minutes = Math.floor(totalSeconds / 60);
        const seconds = Math.floor(totalSeconds % 60);
        return `${minutes.toString().padStart(2, "0")}:${seconds.toString().padStart(2, "0")}`;
    }
}

window.addEventListener("load", function () {
    $("time.countdown").each(function () {
        new Countdown($(this));
    });
});

/**
 * Lazy loader.
 *
 * 1. Add the "lazy-loaded" class to the element.
 * 2. Add the "data-onload" attribute and optionally the "data-priority" attribute.
 *
 * The "data-onload" attribute defines the onload callback,
 * which should have the form `($element) => {}`.
 *
 * The "data-priority" attribute defines when the callback is called,
 * with lower numbers being more important.
 */
window.addEventListener("load", function () {
    const items = $(".lazy-loaded").map(elementToData).get();
    items.sort((a, b) => a.priority - b.priority);
    items.forEach(executeOnLoad);

    function elementToData(_, element) {
        return {
            $element: $(element),
            callback: $(element).data("onload"),
            priority: parseInt($(element).data("priority")) || 5,
        };
    }

    function executeOnLoad(data) {
        window[data.callback](data.$element);
    }
});

/**
 * Seat selector
 *
 * See the associated component and view for the rest of the code.
 * This is the JavaScript for the client-side behavior.
 */
function SeatSelector($root, errorMap) {
    // Properties
    var isSubmitting = false;
    const seats = [];
    var selected = [];

    // Elements
    const $seatMap = $root.find(".interactive-seat-selector");
    const $form = $root.find("form");
    const $selects = $form.find(".form-select");
    const $error = $("<div>")
        .addClass("form-text text-danger d-hidden")
        .insertBefore($form.find(".btn-primary"));

    // Load config
    const maxSelections = $root.data("max-seat-selections");
    const idealMapWidth = $seatMap.data("scaled-for-parent-width");
    const initialMapFontSize = parseFloat($seatMap.css("font-size"));

    // Wire up events
    $(window).on("pageshow", handlePageShow).resize(handleWindowResize);
    handleWindowResize();
    $selects.change(handleSelectChange);
    $form.on("submit", handleFormSubmit);

    // Parse seat map into sorted memory, and add click handler
    $seatMap.find(".seat").each((_, seatElement) => {
        const $seat = $(seatElement);
        const number = $seat.data("number");

        seats[number - 1] = {
            $element: $seat,
            number: number,
            status: $seat.data("status"),
        };

        $seat.click(handleSeatClick);
    });

    // Populate seats into selects now that we have a sorted array to use.
    $.each(seats, (_, seat) => {
        const isAvailable = seat.status === "available";
        $selects.each((_, selectElement) => {
            const $option = $("<option>")
                .data("status", seat.status)
                .prop("disabled", !isAvailable)
                .prop("hidden", !isAvailable)
                .text(seat.number)
                .val(seat.number);
            $(selectElement).append($option);
        });
    });

    // Start the live updates
    try {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/api/watch-seats")
            .build();
        connection.on("SEATS_UPDATED", refreshSeatStatuses);
        connection.start();
        console.log("Started live updates feature.");
    } catch (err) {
        console.error("Unable to start live updates feature.");
    }

    /* Helper functions */

    function deselectSeat(targetSeat) {
        targetSeat.status = "available";
        targetSeat.$element.removeClass("selected").addClass("available");

        selected = selected.filter((s) => s.number !== targetSeat.number);

        $selects.each((_, selectElement) => {
            const $select = $(selectElement);
            if ($select.val() === targetSeat.number.toString()) {
                $select.val("").data("value", "");
                return false;
            }
        });
    }

    function fixDuplicates() {
        $selects.each((ai, a) => {
            $selects.each((bi, b) => {
                if (ai !== bi) {
                    const $b = $(b);
                    if ($(a).val() === $b.val()) {
                        $b.val("").data("value", "");
                    }
                }
            });
        });
    }

    // Pack selects so that the values start at the first one.
    function packSelects() {
        $selects.each((i, selectElement) => {
            const seatNumber = selected[i] ? selected[i].number : "";
            $(selectElement).val(seatNumber).data("value", seatNumber);
        });
    }

    function refreshSeatStatuses(statuses) {
        Object.entries(statuses).forEach(([seatNumber, newStatus]) => {
            const seat = seats[seatNumber - 1];

            const unchanged =
                seat.status === newStatus ||
                (seat.status === "selected" && newStatus === "available");
            if (unchanged) return;

            if (seat.status === "selected") {
                deselectSeat(seat);
                packSelects();
            }

            seat.status = newStatus;
            seat.$element.attr("class", "seat " + newStatus);

            const isAvailable = seat.status === "available";
            $selects.each((_, selectElement) => {
                const $option = $(selectElement).find(`[value=${seatNumber}]`);
                $option
                    .data("status", newStatus)
                    .prop("disabled", !isAvailable)
                    .prop("hidden", !isAvailable);
            });
        });
    }

    function selectSeat(targetSeat) {
        targetSeat.status = "selected";
        targetSeat.$element.removeClass("available").addClass("selected");

        selected.push(targetSeat);
        if (selected.length > maxSelections) {
            deselectSeat(selected.shift());
        }

        // Set the next empty select to the target seat.
        $selects.each((_, selectElement) => {
            const $select = $(selectElement);
            if ($select.val() === "") {
                $select.val(targetSeat.number).data("value", targetSeat.number);
                return false;
            }
        });
    }

    /* Event handling */

    function handleFormSubmit(e) {
        if (!this.checkValidity()) {
            return;
        }

        e.preventDefault();

        if (isSubmitting) return;
        isSubmitting = true;

        $.ajax({
            url: "/api/lock-seats",
            method: "POST",
            data: $form.serialize(),
        })
            .done((responseJSON) => {
                const seatLocks = JSON.stringify(responseJSON);
                setCookie("seatLocks", seatLocks, seatLocks.lockExpiration);
                document.location = "/reservation/new";
            })
            .fail((xhr) => {
                if (xhr.status === 403 /* unauthorized */) {
                    $error.text(errorMap[403][xhr.responseJSON.failureReason]);
                    $error.removeClass("d-hidden");
                } else if (xhr.status === 409 /* conflict */) {
                    const seatNumber = xhr.responseJSON.seatNumber;
                    $error.text(errorMap[409].replace("{}", seatNumber));
                    $error.removeClass("d-hidden");
                }
            })
            .always(() => (isSubmitting = false));
    }

    function handlePageShow() {
        $.getJSON("/api/seat-statuses", refreshSeatStatuses);
    }

    function handleSeatClick() {
        const seat = seats[$(this).data("number") - 1];
        if (seat.status === "selected") {
            deselectSeat(seat);
            packSelects();
        } else if (seat.status === "available") {
            selectSeat(seat);
        }
    }

    function handleSelectChange() {
        const $select = $(this);
        const previousValue = $select.data("value");
        const newValue = $select.val();

        if (newValue) {
            previousValue && deselectSeat(seats[previousValue - 1]);
            selectSeat(seats[newValue - 1]);
            fixDuplicates();
        } else if (previousValue) {
            deselectSeat(seats[previousValue - 1]);
            packSelects();
        }

        $select.data("value", newValue);
    }

    function handleWindowResize() {
        const containerWidth = $seatMap.parent().width();
        if (containerWidth < idealMapWidth) {
            const scale = containerWidth / idealMapWidth;
            const newFontSize = initialMapFontSize * scale;
            $seatMap.css("font-size", newFontSize + "px");
        }
    }
}
