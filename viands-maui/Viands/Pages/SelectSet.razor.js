var SelectSets = {
    dotNetRef: null,
    setDotNetRef: function (dnr) {
        SelectSets.dotNetRef = dnr;
    },
    unMarkAllSelected: function (listElems) {
        console.log(listElems);
    }
}

var SetDotNetRef = SelectSets.setDotNetRef;
var UnMarkAllSelected = SelectSets.unMarkAllSelected;

export {
    SetDotNetRef,
    UnMarkAllSelected
};
