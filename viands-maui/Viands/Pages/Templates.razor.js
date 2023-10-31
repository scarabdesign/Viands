
var Templates = {
    dotNetRef: null,
    setDotNetRef: function (dnr) {
        Templates.dotNetRef = dnr;
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

var SetDotNetRef = Templates.setDotNetRef;
var InitIndex = Templates.initIndex;
var FadeListItem = Templates.fadeListItem;
var Unfade = Templates.unFade;

export {
    SetDotNetRef,
    InitIndex,
    FadeListItem,
    Unfade
};
