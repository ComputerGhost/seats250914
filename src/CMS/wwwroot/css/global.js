/* This file requires Bootstrap, jQuery, and Luxon. */

// All of the text to be translated here:
const CONVERT_TIME_TEXT = "Convert time";

// Moving library access to the top so all of VS' false errors are in one place.
var DateTime = luxon.DateTime;
var createModal = (id) => new bootstrap.Modal(document.getElementById(id));

/*
 * Time conversion feature
 */
$(document).ready(() => {
    var timezoneModal = createModal("timezone-converter-modal");
    var $timestampDisplay = $("#timezone-converter-timestamp");
    var $zonedTimeDisplay = $("#timezone-converter-zonedtime");
    var $timezoneSelect = $("#timezone-converter-timezone");

    function updateZonedTime() {
        var timezone = $timezoneSelect.find("option:selected").text();
        if (timezone.length == 0) {
            $zonedTimeDisplay.text("");
            return;
        }

        var utcTime = DateTime.fromISO($timestampDisplay.text());
        var zonedTime = utcTime.setZone(timezone);
        var displayTime = zonedTime.toFormat("yyyy-MM-dd HH:mm:ss");
        $zonedTimeDisplay.text(displayTime);
    }

    $(Intl.supportedValuesOf("timeZone")).each((_, item) => {
        $timezoneSelect.append($("<option>", { text: item }));
    });

    $timezoneSelect.on("change", updateZonedTime);

    $("time").each(function () {
        var $subject = $(this);

        var $convertButton = $("<button>");
        $convertButton.attr(
            "class",
            "convert-time-button btn btn-sm btn-outline-secondary"
        );
        $convertButton.append($("<i class='bi bi-globe me-2'>"));
        $convertButton.append(CONVERT_TIME_TEXT);

        $convertButton.on("click", () => {
            $timestampDisplay.text($subject.attr("datetime"));
            updateZonedTime();
            timezoneModal.toggle();
        });

        $subject.after($convertButton);
    });
});

