/* This file requires Bootstrap, jQuery, and Luxon. */

// Strings to be translated:
const CHANGE_DISPLAYED_TIME_ZONE = "표시할 시간대를 변경하세요.";

// Moving library access to the top so all of VS' false errors are in one place.
const DateTime = luxon.DateTime;
const createModal = (id) => new bootstrap.Modal(document.getElementById(id));

/*
 * Time conversion feature
 */
$(document).ready(() => {
    const timeZones = Intl.supportedValuesOf("timeZone");
    const currentTimeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;

    const options = timeZones.map((timezone) => {
        return $("<option>").val(timezone).text(timezone);
    });

    $("time").each(function () {
        const $time = $(this);
        const $select = $("<select>")
            .addClass("form-select")
            .attr("aria-label", CHANGE_DISPLAYED_TIME_ZONE);

        options.forEach((option) => $select.append(option.clone()));

        $select.on("change", () => updateDisplayTime($time, $select.val()));

        $select.val(currentTimeZone);
        updateDisplayTime($time, currentTimeZone);

        $time.parent().append($select);
    });

    function updateDisplayTime($time, timeZone) {
        const utcTime = DateTime.fromISO($time.attr("datetime"));
        const zonedTime = utcTime.setZone(timeZone);
        const displayTime = zonedTime.toFormat("yyyy-MM-dd HH:mm:ss");
        $time.text(displayTime);
    }
});
