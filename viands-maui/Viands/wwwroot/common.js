
var globalInteractions = {
    documentBeforeInputCallbacks: {},
    documentInputCallbacks: {},
    documentKeyDownCallbacks: {},
    documentKeyUpCallbacks: {},
    documentKeyPressCallbacks: {},
    documentMouseDownCallbacks: {},
    getCallback: function (ns, cbt) {
        let nscbs = cbt[ns] || [];
        cbt[ns] = nscbs;
        return nscbs;
    },
    applyCallback: function (cbs, e) {
        cbs && Object.keys(cbs).forEach(ns => cbs[ns].length && cbs[ns].forEach(cb => cb && cb(e)))
    },

    addDocumentBeforeInputCallback: function (ns, callback) {
        let nscbs = globalInteractions.getCallback(ns, globalInteractions.documentBeforeInputCallbacks);
        nscbs.push(callback);
    },
    addDocumentInputCallback: function (ns, callback) {
        let nscbs = globalInteractions.getCallback(ns, globalInteractions.documentInputCallbacks);
        nscbs.push(callback);
    },
    addDocumentKeyUpCallback: function (ns, callback) {
        let nscbs = globalInteractions.getCallback(ns, globalInteractions.documentKeyUpCallbacks);
        nscbs.push(callback);
    },
    addDocumentKeyDownCallback: function (ns, callback) {
        let nscbs = globalInteractions.getCallback(ns, globalInteractions.documentKeyDownCallbacks);
        nscbs.push(callback);
    },
    addDocumentKeyPressCallback: function (ns, callback) {
        let nscbs = globalInteractions.getCallback(ns, globalInteractions.documentKeyPressCallbacks);
        nscbs.push(callback);
    },
    addDocumentMouseDownCallback: function (ns, callback) {
        let nscbs = globalInteractions.getCallback(ns, globalInteractions.documentMouseDownCallbacks);
        nscbs.push(callback);
    },

    removeDocumentBeforeInputCallback: function (ns, callback) {
        let nscbs = globalInteractions.getCallback(ns, globalInteractions.documentBeforeInputCallbacks);
        let index = nscbs.indexOf(callback);
        index != -1 && nscbs.splice(index, 1);
    },
    removeDocumentInputCallback: function (ns, callback) {
        let nscbs = globalInteractions.getCallback(ns, globalInteractions.documentInputCallbacks);
        let index = nscbs.indexOf(callback);
        index != -1 && nscbs.splice(index, 1);
    },
    removeDocumentKeyUpCallback: function (ns, callback) {
        let nscbs = globalInteractions.getCallback(ns, globalInteractions.documentKeyUpCallbacks);
        let index = nscbs.indexOf(callback);
        index != -1 && nscbs.splice(index, 1);
    },
    removeDocumentKeyDownCallback: function (ns, callback) {
        let nscbs = globalInteractions.getCallback(ns, globalInteractions.documentKeyDownCallbacks);
        let index = nscbs.indexOf(callback);
        index != -1 && nscbs.splice(index, 1);
    },
    removeDocumentKeyPressCallback: function (ns, callback) {
        let nscbs = globalInteractions.getCallback(ns, globalInteractions.documentKeyPressCallbacks);
        let index = nscbs.indexOf(callback);
        index != -1 && nscbs.splice(index, 1);
    },
    removeDocumentMouseDownCallback: function (ns, callback) {
        let nscbs = globalInteractions.getCallback(ns, globalInteractions.documentMouseDownCallbacks);
        let index = nscbs.indexOf(callback);
        index != -1 && nscbs.splice(index, 1);
    },

    removeDocumentCallbackForNS: function (ns) {
        let binscbs = globalInteractions.getCallback(ns, globalInteractions.documentBeforeInputCallbacks);
        binscbs && binscbs.length && binscbs.forEach(cb => globalInteractions.removeDocumentInputCallback(ns, cb));
        let inscbs = globalInteractions.getCallback(ns, globalInteractions.documentInputCallbacks);
        inscbs && inscbs.length && inscbs.forEach(cb => globalInteractions.removeDocumentInputCallback(ns, cb));
        let kunscbs = globalInteractions.getCallback(ns, globalInteractions.documentKeyUpCallbacks);
        kunscbs && kunscbs.length && kunscbs.forEach(cb => globalInteractions.removeDocumentKeyUpCallback(ns, cb));
        let kdnscbs = globalInteractions.getCallback(ns, globalInteractions.documentKeyDownCallbacks);
        kdnscbs && kdnscbs.length && kdnscbs.forEach(cb => globalInteractions.removeDocumentKeyDownCallback(ns, cb));
        let kpnscbs = globalInteractions.getCallback(ns, globalInteractions.documentKeyPressCallbacks);
        kpnscbs && kpnscbs.length && kpnscbs.forEach(cb => globalInteractions.removeDocumentKeyPressCallback(ns, cb));
        let mdnscbs = globalInteractions.getCallback(ns, globalInteractions.documentMouseDownCallbacks);
        mdnscbs && mdnscbs.length && mdnscbs.forEach(cb => globalInteractions.removeDocumentMouseDownCallback(ns, cb));
    },

    documentBeforeInput: function (e) {
        globalInteractions.applyCallback(globalInteractions.documentBeforeInputCallbacks, e);
    },
    documentInput: function (e) {
        globalInteractions.applyCallback(globalInteractions.documentInputCallbacks, e);
    },
    documentKeyUp: function (e) {
        globalInteractions.applyCallback(globalInteractions.documentKeyUpCallbacks, e);
    },
    documentKeyDown: function (e) {
        globalInteractions.applyCallback(globalInteractions.documentKeyDownCallbacks, e);
    },
    documentKeyPress: function (e) {
        globalInteractions.applyCallback(globalInteractions.documentKeyPressCallbacks, e);
    },
    documentMouseDown: function (e) {
        globalInteractions.applyCallback(globalInteractions.documentMouseDownCallbacks, e);
    }
}

var tearDownGCBForNS = function (ns) {
    globalInteractions.removeDocumentCallbackForNS(ns);
}

var adjustModalForStatusBar = function () {
    let statusBar = document.querySelector(".status-bar-safe-area");
    let modal = document.querySelector(".rz-dialog");
    if (statusBar && modal) {
        let newMargin = statusBar.offsetHeight + 40;
        if (modal.offsetTop < newMargin) {
            modal.style.marginTop = newMargin + "px";
        }
    }
}

var adjustSettingsWindow = function () {
    let _setModalSize = function () {
        let settingsModal;
        if (settingsModal = document.querySelector(".settings_modal")) {
            settingsModal.style.setProperty("width", (window.innerWidth - (window.innerWidth <= 400 ? 0 : 116)) + "px");
            settingsModal.style.setProperty("height", (window.innerHeight - (window.innerHeight <= 400 ? 0 : 60)) + "px");
        }
    }
    setTimeout(_setModalSize);
    finalResize && clearTimeout(finalResize);
    finalResize = setTimeout(_setModalSize, 1000);
}

var finalResize;
var windowResized = function (e) {
    [...new Set(document.getElementsByClassName("lists_container"))].forEach(
        listitem => listitem.style.setProperty("width", (window.innerWidth - (window.innerWidth > 640 ? 100 : 0)) + "px")
    );
    adjustSettingsWindow();
    adjustPageHeightForFocus();
    adjustMenuEditor();
}

var adjustMenuEditor = function () {
    if (!!document.querySelector(".viands_menu_editor")) {
        document.querySelector(".inset_page").style.setProperty("height", ((window.innerHeight / 25) + 60) + "vh")
    }
}

var scrollToLastSelected = function () {
    let ls = document.querySelector(".last_selected");
    ls && ls.scrollIntoViewIfNeeded();
}

var removeLastSelected = function () {
    document.querySelectorAll(".last_selected").forEach(elem => {
        elem.classList && elem.classList.remove("last_selected");
    });
};

var focusNewInput = function () {
    let input = document.querySelector(".new_list_item");
    focusTargetElement(input);
};

var focusTargetElement = function (input) {
    input && input.focus();
}

var highlightAllText = function (input) {
    input && input.focus();
    input && input.select();
}

var highlightText = function (input, start, end) {
    input && input.focus();
    input.setSelectionRange(start, end);
}

var cursorToStart = function (input) {
    input && input.focus();
    input && input.setSelectionRange(0, 0);
}

var cursorToEnd = function (input) {
    input.setSelectionRange(9999, 9999);
}

var scrollToFocusedElementTimeout;
var cancelScrollToElement = function () {
    scrollToFocusedElementTimeout && clearTimeout(scrollToFocusedElementTimeout);
    scrollToFocusedElementTimeout = setTimeout(() => adjustPageHeightForFocus(false), 300);
}

var adjustPageHeightForFocusTimeout;
var adjustPageHeightForFocus = function (onOff) {
    let listSpacer = document.getElementById("list_spacer");
    if (onOff) {
        listSpacer && (listSpacer.style.height = "900px");
        adjustPageHeightForFocusTimeout && clearTimeout(adjustPageHeightForFocusTimeout);
        document.querySelector("html").style.overflow = "initial"
    }
    else {
        listSpacer && (listSpacer.style.height = "200px");
        document.querySelector("html").scrollTop = 0;
        document.querySelector("html").scrollLeft = 0;
        adjustPageHeightForFocusTimeout && clearTimeout(adjustPageHeightForFocusTimeout);
        adjustPageHeightForFocusTimeout = setTimeout(() => {
            document.querySelector("html").style.overflow = "hidden"
        }, 500);
    }
}

var scrollToFocusedElementTimeout;
var scrollToFocusedElementAfterDelay = function () {
    scrollToFocusedElementTimeout && clearTimeout(scrollToFocusedElementTimeout);
    scrollToFocusedElementTimeout = setTimeout(scrollToFocusedElement, 1000);
}

var scrollToFocusedElement = function () {
    if (!document.activeElement) return;
    let tag = document.activeElement.tagName.toLowerCase();
    if (tag == "textarea" || tag == "input")
        scrollToElement(document.activeElement);
}

var scrollToElement = function (input) {
    if (!input) return;
    let offSet = parseInt(input.dataset.offset);
    if (offSet && offSet > 0) {
        adjustPageHeightForFocus(true);
        return setTimeout(() => scrollToY(offSet));
    }
    adjustPageHeightForFocus(true);
    input && input.scrollIntoViewIfNeeded();
    //if (input && input.getBoundingClientRect) {
    //    document.getElementsByTagName("html")[0].scrollTo(
    //        0, input.getBoundingClientRect().top + input.getBoundingClientRect().height
    //    )
    //}
}

var scrollToY = function (y) {
    document.getElementsByTagName("html")[0].scrollTop = y;
}

var checkFocus = function (textinput) {
    let txHasFocus = document.activeElement && document.activeElement.classList &&
    (
        document.activeElement.classList.contains("list_item_name_input") ||
        document.activeElement.classList.contains("list_name") ||
        document.activeElement.classList.contains("list_desc") ||
        document.activeElement.classList.contains("edit_producttype_input") ||
        document.activeElement.classList.contains("product_filter") ||
        document.activeElement.classList.contains("location_filter") || 
        document.activeElement.classList.contains("rz-inputtext")
    );
    if (txHasFocus) {
        scrollToElement(textinput);
    } else {
        cancelScrollToElement();
    }
}

var isCurrentFocus = function (element) {
    return document.activeElement == element;
}

var sendDotNetMessage = function (method, message) {
    DotNet && DotNet.invokeMethodAsync("Viands", "OnJavaScriptMessage", JSON.stringify({
        "Method": method, 
        "Message": message
    }));
}