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
 * To use, simply add the `countdown` CSS class to a `<time`> element that has
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
function SeatSelector($element, errorMap) {
    const that = this;
    this._$reservationPageUrl = $element.data("reservation-page-url");

    const seatForm = new SeatForm($element.find("form"));
    const seatMap = new SeatMap($element.find(".interactive-seat-selector"));

    // The seat map and the select need to be in sync.
    seatForm.onChange = (seatNumber) => seatMap.selectSeat(seatNumber - 1);
    seatMap.onInitialized = (seats) => seatForm.addSeats(seats);
    seatMap.onSelected = (seat) => seatForm.selectSeat(seat.index + 1);
    seatMap.onDeselected = () => seatForm.selectSeat(null);

    // Calling this will load the seats from the DOM to finish initializing.
    seatMap.build();

    seatForm.onConflict = function (requestData) {
        seatForm.removeSeat(requestData.seatNumber);
        seatMap.deselectSeat(requestData.seatNumber - 1);
        seatMap.updateSeat(requestData.seatNumber - 1, "on-hold");
    };

    seatForm.onSuccess = function (requestData, responseData) {
        const seatLock = JSON.stringify(responseData);
        setCookie("seatLock", seatLock, responseData.lockExpiration);
        document.location = that._$reservationPageUrl;
    };

    var realTimeUpdates = new RealTimeSeatUpdates();
    realTimeUpdates.onSeatRefreshed = function (seatNumber, newStatus) {
        const updatedSeat = seatMap.updateSeat(seatNumber - 1, newStatus);
        if (updatedSeat) {
            seatForm.updateSeat(updatedSeat);
        }
    };
    realTimeUpdates.start();

    // This encapsulates the form logic.
    function SeatForm($formElement) {
        const that = this;

        this._$formElement = $formElement;
        this._$selectElement = $formElement.find("select");
        this._$errorElement = $("<div>")
            .addClass("form-text text-danger")
            .insertAfter(this._$selectElement);

        this.onChange = null;
        this.onConflict = null;
        this.onSuccess = null;

        this._$selectElement.change(() => {
            const seatNumber = that._$selectElement.val();
            that.onChange && that.onChange(seatNumber);
        });

        this._$formElement.on("submit", function (e) {
            if (this.checkValidity()) {
                e.preventDefault();
                that.submit();
            }
        });

        this.addSeats = function (seats) {
            $.each(seats, function (_, seat) {
                if (seat.status === "available") {
                    const $optionElement = $("<option>")
                        .val(seat.index + 1)
                        .text(seat.$element.text());
                    that._$selectElement.append($optionElement);
                }
            });
        };

        this.removeSeat = function (seatNumber) {
            const seat = that._$selectElement.find(`[value=${seatNumber}]`);
            seat && seat.remove();
        };

        this.selectSeat = function (seatNumber) {
            that._$selectElement.val(seatNumber);
        };

        this.submit = function () {
            const formData = new FormData(that._$formElement[0]);
            const formEntries = formData.entries();
            const requestData = Object.fromEntries(formEntries);

            $.ajax({
                url: $formElement.attr("action"),
                method: $formElement.attr("method").toUpperCase(),
                contentType: "application/json",
                data: JSON.stringify(requestData),
            })
                .done(function (responseData) {
                    that._handleSuccess(requestData, responseData);
                })
                .fail(function (xhr) {
                    if (xhr.status === 403) {
                        that._handleUnauthorized(xhr.responseJSON);
                    } else if (xhr.status === 409) {
                        that._handleConflict(requestData);
                    }
                });
        };

        this.updateSeat = function (seat) {
            if (seat.status !== "available") {
                that.removeSeat(seat.index + 1);
            } else {
                var $nextOption;
                that._$selectElement.children().each(function () {
                    $nextOption = $(this);
                    return $nextOption.text() <= seat.$element.text();
                });

                const $optionElement = $("<option>")
                    .val(seat.index + 1)
                    .text(seat.$element.text());

                if (seat.$element.text() === $nextOption.text()) {
                    $nextOption.replaceWith($optionElement);
                } else {
                    $optionElement.insertBefore($nextOption);
                }
            }
        };

        this._handleUnauthorized = function (responseJson) {
            const template = errorMap[403][responseJson.failureReason];
            that._$errorElement.text(template);
        };

        this._handleConflict = function (requestData) {
            const template = errorMap[403];
            that._$errorElement.text(
                template.replace("{}", requestData.seatNumber)
            );
            that.onConflict && that.onConflict(requestData);
        };

        this._handleSuccess = function (requestData, responseData) {
            that.onSuccess && that.onSuccess(requestData, responseData);
        };
    }

    // This encapsulates the seat map logic.
    function SeatMap($mapElement) {
        const that = this;

        this._initialFontSize = parseFloat($mapElement.css("font-size"));
        this._idealParentWidth = $mapElement.data("scaled-for-parent-width");
        this._seats = [];
        this._selection = null;

        // Set these before calling `build`.
        this.onInitialized = null;
        this.onSelected = null;
        this.onDeselected = null;

        // Load the seat map into memory and tie up events.
        this.build = function () {
            $mapElement.find(".seat").each((_, seatElement) => {
                var $seatElement = $(seatElement);
                that._addSeatToMemory($seatElement);
                that._addSeatClickHandler($seatElement);
            });

            $(window).resize(() => that._scaleToFit());
            that._scaleToFit();

            that.onInitialized && that.onInitialized(that._seats);
            $mapElement.addClass("initialized");
        };

        this.deselectSeat = function (index) {
            var seat = that._seats[index];
            if (!seat || seat.status !== "selected") {
                return;
            }

            seat.status = "available";
            seat.$element.removeClass("selected");
            seat.$element.addClass("available");

            that._selection = null;

            that.onDeselected && that.onDeselected(seat);
        };

        this.selectSeat = function (index) {
            var seat = that._seats[index];
            if (!seat) {
                that._selection && that.deselectSeat(that._selection.index);
                return;
            } else if (seat.status !== "available") {
                return;
            }

            seat.status = "selected";
            seat.$element.removeClass("available");
            seat.$element.addClass("selected");

            that._selection && that.deselectSeat(that._selection.index);
            that._selection = seat;

            that.onSelected && that.onSelected(seat);
        };

        this.updateSeat = function (index, newStatus) {
            const seat = that._seats[index];
            if (!seat) {
                return false;
            }

            if (seat.status === "selected") {
                if (newStatus === "available") {
                    return false;
                } else {
                    that.deselectSeat(index);
                }
            }

            seat.status = newStatus;
            seat.$element.attr("class", "seat " + newStatus);
            return seat;
        };

        this._addSeatToMemory = function ($seatElement) {
            var index = $seatElement.data("index");
            var status = $seatElement.data("status");
            that._seats[index] = {
                index: index,
                status: status,
                $element: $seatElement,
            };
        };

        this._addSeatClickHandler = function ($seatElement) {
            $seatElement.click(function () {
                var index = $(this).data("index");
                var seat = that._seats[index];
                if (seat.status === "selected") {
                    that.deselectSeat(index);
                } else if (seat.status === "available") {
                    that.selectSeat(index);
                }
            });
        };

        this._scaleToFit = function () {
            var parentWidth = $mapElement.parent().width();
            if (parentWidth < that._idealParentWidth) {
                var scale = parentWidth / that._idealParentWidth;
                var newFontSize = that._initialFontSize * scale;
                $mapElement.css("font-size", newFontSize + "px");
            }
        };
    }

    // This handles the real time seat updates logic.
    function RealTimeSeatUpdates() {
        const that = this;
        this.onSeatRefreshed = null;

        this._connection = new signalR.HubConnectionBuilder()
            .withUrl("/api/watch-seats")
            .build();
        this._connection.on("SEATS_UPDATED", (seatStatuses) =>
            this._onSeatsRefreshed(seatStatuses)
        );

        window.addEventListener("pageshow", () => that.reloadStatuses());

        this.reloadStatuses = async function () {
            if (that.onSeatRefreshed !== null) {
                $.getJSON("/api/seat-statuses", (statuses) =>
                    this._onSeatsRefreshed(statuses)
                );
            }
        };

        this.start = async function () {
            try {
                await that._connection.start();
                console.log("Connected to `watch-seats` endpoint.");
            } catch (err) {
                console.error(err);
            }
        };

        this._onSeatsRefreshed = function (seatStatuses) {
            if (that.onSeatRefreshed !== null) {
                Object.entries(seatStatuses).forEach(
                    ([seatNumber, newStatus]) => {
                        that.onSeatRefreshed(seatNumber, newStatus);
                    }
                );
            }
        };
    }
}
