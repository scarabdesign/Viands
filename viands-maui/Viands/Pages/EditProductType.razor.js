
var EditProductType = {
    dotNetRef: null,
    savePriceTimeout: null,
    setDotNetRef: function (dnr) {
        EditProductType.dotNetRef = dnr;
        EditProductType.tearDown();
        EditProductType.setUp();
    },
    setUp: function () {
        let priceInput = document.querySelector(".producttype_price");
        if (priceInput) {
            priceInput.addEventListener("keydown", EditProductType.priceOnKeyDown);
            priceInput.addEventListener("keyup", EditProductType.priceOnKeyUp);
            priceInput.addEventListener("mouseup", EditProductType.priceOnMouseUpFocus);
            priceInput.addEventListener("focus", EditProductType.priceOnMouseUpFocus);
        }
    },
    selectTypeNameInputText: function () {
        let input = document.querySelector(".edit_producttype_input");
        input && highlightAllText(input);
    },
    priceOnKeyDown: function(e) {
        if (!EditProductType.backspacePrice(this, e)) {
            EditProductType.adjustPriceForDisplay(this, e);
        }
    },
    priceOnKeyUp: function (e) {
        EditProductType.storeCurrentPrice();
    },
    priceOnMouseUpFocus: function (e) {
        EditProductType.cursorToEnd(this, e);
        EditProductType.formatForDisplay(this);
    },
    tearDown: function () {
        let priceInput = document.querySelector(".producttype_price");
        if (priceInput) {
            priceInput.removeEventListener("keydown", EditProductType.priceOnKeyDown);
            priceInput.removeEventListener("keyup", EditProductType.priceOnKeyUp);
            priceInput.removeEventListener("mouseup", EditProductType.priceOnMouseUpFocus);
            priceInput.removeEventListener("focus", EditProductType.priceOnMouseUpFocus);
        }
    },
    getPriceValue: function () {
        let priceInput = document.querySelector(".producttype_price");
        return priceInput && priceInput.value || 0;
    },
    makeDirty: function () {
        EditProductType.dotNetRef && EditProductType.dotNetRef.invokeMethodAsync("MakeDirty", null);
    },
    storeCurrentPrice: function () {
        clearTimeout(EditProductType.savePriceTimeout);
        EditProductType.savePriceTimeout = setTimeout(() => EditProductType.dotNetRef && EditProductType.dotNetRef.invokeMethodAsync("StoreCurrentPrice", null), 1000);
    },
    checkModalFocus: function (scrollto) {
        let txHasFocus = document.activeElement && document.activeElement.classList &&
        (
            document.activeElement.classList.contains("edit_producttype_input") ||
            document.activeElement.classList.contains("producttype_location")
        );
        if (txHasFocus && window.outerHeight <= 1000) {
            scrollto && EditProductType.scrollToModalInput(scrollto);
            EditProductType.adjustPageHeightForFocus(true);
        } else {
            cancelScrollToElement();
            EditProductType.adjustPageHeightForFocus(false);
        }
    },
    scrollToY: function () {
        input.getBoundingClientRect().top + input.getBoundingClientRect().height
    },
    scrollToModalInput: function (elem) {
        let modalScroller = document.querySelector(".editproducttype_modal .rz-dialog-content");
        modalScroller && modalScroller.scrollTo(0, elem.offsetTop);
    },
    cursorToEnd: function (input, event) {
        input.type = "text";
        input.setSelectionRange(9999, 9999);
        input.type = "number";
        event && event.preventDefault();
    },
    backspacePrice: function (input, event) {
        if (event.key == "Backspace") {
            input.value = input.value.slice(0, -1);
            EditProductType.formatForDisplay(input);
            event.preventDefault();
            return true;
        }
    },
    formatForDisplay: function (input, plus) {
        if (!input.value) return;
        let stripped = input.value.replace(".", "") + (plus || "");
        let newVal = stripped.slice(0, -2) + "." + stripped.slice(-2);
        input.value = parseFloat(newVal).toFixed(2);
        input.setAttribute("data-isZero", input.value == 0);
    },
    adjustPriceForDisplay: function (input, event) {
        if (input) {
            EditProductType.cursorToEnd(input);
            if (input.value) {
                if (event && parseInt(event.key) == event.key) {
                    EditProductType.formatForDisplay(input, event.key);
                } else {
                    input.value = parseFloat(input.value).toFixed(2);
                }
                event &&
                    event.key != "Backspace" &&
                    event.key != "Tab" &&
                    event.preventDefault();
            }
        }
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

var SetDotNetRef = EditProductType.setDotNetRef;
var CheckModalFocus = EditProductType.checkModalFocus;
var AdjustPriceForDisplay = EditProductType.adjustPriceForDisplay;
var GetPriceValue = EditProductType.getPriceValue;
var SelectTypeNameInputText = EditProductType.selectTypeNameInputText;

export { SetDotNetRef, CheckModalFocus, AdjustPriceForDisplay, GetPriceValue, SelectTypeNameInputText }