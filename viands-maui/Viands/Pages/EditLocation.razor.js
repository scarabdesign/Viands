
var EditLocation = {
    dotNetRef: null,
    setDotNetRef: function (dnr) {
        EditLocation.dotNetRef = dnr;
        EditLocation.tearDown();
        EditLocation.setUp();
    },
    setUp: function () {

    },
    tearDown: function () {

    },
    makeDirty: function () {
        EditLocation.dotNetRef && EditLocation.dotNetRef.invokeMethodAsync("MakeDirty", null);
    },
    checkModalFocus: function (scrollto) {
        let txHasFocus = document.activeElement && document.activeElement.classList &&
        (
            document.activeElement.classList.contains("edit_location_input")
        );
        if (txHasFocus && window.outerHeight <= 400) {
            scrollto && EditLocation.scrollToModalInput(scrollto);
            EditLocation.adjustPageHeightForFocus(true);
        } else {
            cancelScrollToElement();
            EditLocation.adjustPageHeightForFocus(false);
        }
    },
    scrollToY: function () {
        input.getBoundingClientRect().top + input.getBoundingClientRect().height
    },
    scrollToModalInput: function (elem) {
        let modalScroller = document.querySelector(".editlocation_modal .rz-dialog-content");
        modalScroller && modalScroller.scrollTo(0, elem.offsetTop);
    },
    adjustPageHeightForFocus: function (onOff) {
        let listSpacer = document.getElementById("form_spacer");
        if (onOff) {
            listSpacer && (listSpacer.style.height = "900px");
        }
        else {
            listSpacer && (listSpacer.style.height = "200px");
        }
    }
}

var SetDotNetRef = EditLocation.setDotNetRef;
var CheckModalFocus = EditLocation.checkModalFocus;

export { SetDotNetRef, CheckModalFocus }