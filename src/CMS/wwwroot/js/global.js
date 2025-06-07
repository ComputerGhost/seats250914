/* This file requires Bootstrap, jQuery, and Luxon. */

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
            .attr("aria-label", "Change displayed time zone.");

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
