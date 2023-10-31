
var ProductSets = {
    dotNetRef: null,
    setDotNetRef: function (dnr) {
        ProductSets.dotNetRef = dnr;
    },
    fadeListItem: function (deElem, outIn) {
        if (deElem && deElem.classList) {
            deElem.classList.add("fade");
            if (!outIn) {
                deElem.classList.add("out");
            }
            else {
                deElem.classList.add("in");
            }
        }
    },
    unFade: function (deElem) {
        if (deElem && deElem.classList) {
            deElem.classList.remove("fade");
            deElem.classList.remove("out");
            deElem.classList.remove("in");
        }
    }
}

var SetDotNetRef = ProductSets.setDotNetRef;
var InitIndex = ProductSets.initIndex;
var FadeListItem = ProductSets.fadeListItem;
var Unfade = ProductSets.unFade;

export {
    SetDotNetRef,
    InitIndex,
    FadeListItem,
    Unfade
};
