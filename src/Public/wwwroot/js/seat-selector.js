function SeatSelector ($mapElement)
{
    var that = this;
    this.maxSeats = 1;
    this.onInitialized = null;
    this.onSelected = null;
    this.onDeselected = null;

    this._initialFontSize = parseFloat($mapElement.css("font-size"));
    this._looksGoodAtParentWidth = $mapElement.data("scaled-for-parent-width");
    this._$mapElement = $mapElement;
    this._seats = [];
    this._selections = [];

    this.build = function () {
        that._$mapElement.find(".seat").each((_, seatElement) => {
            var $seatElement = $(seatElement);
            var index = $seatElement.data("index");
            var status = $seatElement.data("status");

            that._seats[index] = {
                index: index,
                status: status,
                $element: $seatElement,
            };

            $seatElement.click(function () {
                var index = $(this).data("index");
                var seat = that._seats[index];
                if (seat.status === "selected") {
                    that.deselectSeat(index);
                } else if (seat.status === "available") {
                    that.selectSeat(index);
                }
            });
        });

        $(window).resize(() => that._scaleToFit());
        that._scaleToFit();

        that.onInitialized && that.onInitialized(that._seats);
    };

    this.selectSeat = function (index) {
        var seat = that._seats[index];
        if (seat.status !== "available") {
            return;
        }

        seat.status = "selected";
        seat.$element.removeClass("available");
        seat.$element.addClass("selected");

        that._selections.push(seat);
        if (that._selections.length > that.maxSeats) {
            var previousSeat = that._selections.shift();
            that.deselectSeat(previousSeat.index);
        }

        that.onSelected && that.onSelected(seat);
    };

    this.deselectSeat = function (index) {
        var seat = that._seats[index];
        if (seat.status !== "selected") {
            return;
        }

        seat.status = "available";
        seat.$element.removeClass("selected");
        seat.$element.addClass("available");

        that._selections = that._selections.filter(
            (selectedSeat) => selectedSeat.index !== index
        );

        that.onDeselected && that.onDeselected(seat);
    };

    this._scaleToFit = function () {
        var parentWidth = that._$mapElement.parent().width();
        console.log(parentWidth);
        if (parentWidth < that._looksGoodAtParentWidth) {
            var scale = parentWidth / that._looksGoodAtParentWidth;
            var newFontSize = that._initialFontSize * scale;
            that._$mapElement.css("font-size", newFontSize + "px");
        }
    };
}

// Tie-in to component HTML.
window.addEventListener("load", function () {
    var $mapElement = $("#seat-map");
    var $selectElement = $("#seat");

    var seatSelector = new SeatSelector($mapElement);

    seatSelector.onInitialized = function (seats) {
        $.each(seats, function (_, seat) {
            if (seat.status === "available") {
                var $optionElement = $("<option>")
                    .val(seat.index)
                    .text(seat.$element.text());
                $selectElement.append($optionElement);
            }
        });
    };

    seatSelector.onSelected = function (seat) {
        $selectElement.val(seat.index);
    };

    seatSelector.onDeselected = function () {
        $selectElement.val(null);
    };

    seatSelector.build();

    $selectElement.change(() => {
        var index = $selectElement.val();
        seatSelector.selectSeat(index);
    });
});
