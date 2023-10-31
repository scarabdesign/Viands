
var EditLocations = {
    newProductTypeAdded: false,
    dotNetRef: null,
    setDotNetRef: function (dnr) {
        EditLocations.dotNetRef = dnr;
    },
    scrollToY: function (y) {
        document.getElementsByTagName("html")[0].scrollTop = y
    },
    scrollToItem: function (elem, fast = false, where = "center") {
        elem && elem.scrollIntoView({
            behavior: fast ? "instant" : "smooth",
            block: where
        });
    },
    sendDotNetMessage: function (method, listitemid, producttypeid, stringval) {
        let message = {
            "ListItemId": parseInt(listitemid)
        };
        if (producttypeid != null) {
            message["ProductTypeId"] = parseInt(producttypeid);
        }
        if (stringval != null) {
            message["CurrentVal"] = stringval.replace(/"/g, "\"");
        }
        EditLocations.dotNetRef && EditLocations.dotNetRef.invokeMethodAsync(method, JSON.stringify(message));
    },
    keyOnDocument: function (e) {
        if (e.key == "Escape") {

        }
    }
}

var SetDotNetRef = EditLocations.setDotNetRef;

export {
    SetDotNetRef
};
