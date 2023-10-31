
var Index = {
    dotNetRef: null,
    setDotNetRef: function (dnr) {
        Index.dotNetRef = dnr;
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

var SetDotNetRef = Index.setDotNetRef;
var InitIndex = Index.initIndex;
var FadeListItem = Index.fadeListItem;
var Unfade = Index.unFade;

export {
    SetDotNetRef,
    InitIndex,
    FadeListItem,
    Unfade
};
