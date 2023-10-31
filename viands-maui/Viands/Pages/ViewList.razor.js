
var ViewList =  {
    productTypesShown: false,
    newListItemAdded: false,
    dotNetRef: null,
    setDotNetRef: function (dnr) {
        ViewList.dotNetRef = dnr;
    },
    initViewList: function () {
        ViewList.initListItemEditor();
        document.querySelectorAll(".quantity_input .rz-inputtext").forEach(elem => {
            ViewList._resizeTextInputToFit(elem);
        });
    },
    getCurrentInput: function () {
        let s = window.getSelection();
        if (s.anchorNode && s.anchorNode.tagName) {
            if (s.anchorNode.tagName.toLowerCase() == "input") {
                return s.anchorNode;
            }
            let input = s.anchorNode.querySelector(":scope > .list_item_name_input");
            if (input) {
                return input;
            }
        }
    },
    getPTSel: function (listitemid) {
        return document.querySelector(".producttypeselector[data-listitemid='" + listitemid + "']");
    },
    clearProductFilter: function (listitemid) {
        let ps = ViewList.getPTSel(listitemid);
        ps && ps.querySelectorAll(".producttypeitem").forEach(pt => pt.classList.remove("hide"));
    },
    enableDisableTabIndices: function (onOff) {
        let inputs = document.querySelectorAll(".list_item_name_input");
        inputs && inputs.forEach((inp, ind) => {
            if (!onOff) {
                inp["data-tabindex"] = inp.tabIndex;
                inp.tabIndex = -1;
            } else {
                inp.tabIndex = inp["data-tabindex"] != null ? inp["data-tabindex"] : ind;
            }
        });
    },
    resizeTextInputToFit: function () {
        ViewList._resizeTextInputToFit(this);
    },
    _resizeTextInputToFit: function (input) {
        let val = input.value;
        let newSize = "1";
        if (val.length > 3) {
            newSize = "0.7"
        }
        if (val.length > 4) {
            newSize = "0.5"
        }
        if (val.length > 5) {
            newSize = "0.4"
        }

        input.style.fontSize != (newSize + "rem") && (input.style.fontSize = newSize + "rem");
    },
    saveCurrentQuantity: function () {
        let listitemid = this.closest(".lists_item").dataset.listitemid;
        ViewList.sendDotNetMessage("StoreCurrentQuantityJS", listitemid, null, this.value);
    },
    filterProductTypes: function (input) {
        let [listitemid, producttypeid] = ViewList.getEntryAttrs(input);
        ViewList.clearProductFilter(listitemid);
        ViewList.showProductTypes(input);
        let inputText = input.value && input.value.toLowerCase();
        if (!inputText) return;
        let count = 0;
        let ps = ViewList.getPTSel(listitemid);
        let selected;
        ps && ps.querySelectorAll(".producttypeitem").forEach(pt => {
            let tarText = pt.innerText && pt.innerText.toLowerCase();
            let result = tarText && tarText.search(new RegExp(inputText, "gi"));
            result == -1 || !inputText ? pt.classList.add("hide") : pt.classList.remove("hide");
            if (!selected && result === 0) {
                selected = pt;
            }

            if (tarText === inputText) {
                selected = pt;
            }

            count += pt.classList.contains("hide") ? 0 : 1;
        });
        if (count == 0) {
            ViewList.dismissProductTypes();
        }
        else {
            selected && ViewList.highlightSelectorItem(selected);
        }
    },
    removePrevSelection: function () {
        let selected = document.querySelector(".producttypeitem.selected")
        selected && selected.classList && selected.classList.remove("selected");
    },
    dismissProductTypes: function () {
        ViewList.removePrevSelection();
        document.querySelectorAll(".producttypeselector").forEach(ps => ps && ps.classList.remove("show"));
        ViewList.productTypesShown = false;
    },
    getEntryAttrs: function (nameInput) {
        if (!nameInput) return;
        return [
            nameInput.dataset.listitemid,
            nameInput.dataset.producttypeid
        ];
    },
    highlightFirstSelectorItem: function (listitemid) {
        let ps = ViewList.getPTSel(listitemid);
        let typeList = ps && [...new Set(ps.querySelectorAll(".producttypeitem:not(.hide)"))];
        typeList && typeList.length && ViewList.highlightSelectorItem(typeList[0]);
    },
    highlightNextItem: function (input, upDown) {
        let [listitemid, producttypeid] = ViewList.getEntryAttrs(input);
        let ps = ViewList.getPTSel(listitemid);
        let current = ps && ps.querySelectorAll(".producttypeitem.selected")[0];
        let typeList = [...new Set(ps.querySelectorAll(".producttypeitem:not(.hide)"))];
        let index = current && typeList.indexOf(current) || -1;
        if (index == -1) index = 0;
        let next = index + (upDown ? -1 : 1);
        if (next <= -1) {
            next = typeList.length - 1;
        }
        if (next > typeList.length - 1) {
            next = 0;
        }
        ViewList.highlightSelectorItem(typeList[next], true);
        current && current.classList.remove("selected");
    },
    highlightSelectorItem: function (selectorItem, follow) {
        if (!selectorItem || !selectorItem.classList) return;
        selectorItem.classList.add("selected");
        follow && ViewList.scrollToItem(selectorItem, true, "nearest");
    },
    changeProductSelection: function (listitemid, producttypeid) {
        ViewList.removePrevSelection();
        let ps = ViewList.getPTSel(listitemid);
        let currentSelection = ps && ps.querySelector(".producttypeitem[data-producttypeid='" + producttypeid + "']")
        ViewList.highlightSelectorItem(currentSelection);
        return !!currentSelection;
    },
    showProductTypes: function (nameInput) {
        if (!nameInput) return;
        let [listitemid, producttypeid] = ViewList.getEntryAttrs(nameInput);
        ViewList.changeProductSelection(listitemid, producttypeid);
        let ps = ViewList.getPTSel(listitemid);
        if (!!ps) {
            ps.style = "width:" + (nameInput.offsetWidth + 35) + "px;";
            ps.classList.add("show");
            let selected = ps.querySelector(".producttypeitem.selected");
            selected && ViewList.scrollToItem(selected);
            ViewList.productTypesShown = true;
        }
    },
    getMainOptName: function (optElem) {
        let optText = optElem.innerHTML.trim();
        let optStripped = optText && optText.match(/(?:(?!\().)*/i);
        if (optStripped && optStripped.length) {
            return optStripped[0].trim();
        }
    },
    focusLastListItem: function () {
        let input = [...new Set(document.getElementsByClassName("list_item_name_input"))]
            .sort((e1, e2) => e2.tabIndex - e1.tabIndex)[0];
        focusTargetElement(input);
    },
    scrollToItem: function (elem, fast = false, where = "center") {
        elem && elem.scrollIntoView({
            behavior: fast ? "instant" : "smooth",
            block: where
        });
    },
    blurTextInput: function () {
        document.activeElement.blur();
    },
    itemNameOnFocus: function () {
        checkFocus(this);
    },
    itemNameOnBlur: function () {
        let input = this;
        setTimeout(() => {
            let _addnew = function () {
                ViewList.addNewListItem();
                ViewList.newListItemAdded = false;
            }
            if (document.activeElement != input) {
                if (!input.value) {
                    if (!ViewList.newListItemAdded) {
                        ViewList.filterEmpty();
                    } else {
                        input.focus();
                    }
                    return checkFocus();
                }
                ViewList.confirmSaveCurrentValue(input);
            }
            if (ViewList.newListItemAdded) {
                return setTimeout(_addnew, 300);
            }

            checkFocus();
        }, 300);
    },
    filterEmpty: function () {
        ViewList.sendDotNetMessage("FilterEmpty");
    },
    prepAddNewListItem: function () {
        ViewList.newListItemAdded = true;
        document.activeElement.blur();
    },
    addNewListItem: function () {
        let oldnew = document.querySelector(".list_item_name_input.new_list_item");
        oldnew && oldnew.classList && oldnew.classList.remove("new_list_item");
        ViewList.sendDotNetMessage("AddNewListItem");
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
        ViewList.dotNetRef && ViewList.dotNetRef.invokeMethodAsync(method, JSON.stringify(message));
    },
    confirmSaveCurrentValue: function (input) {
        let listitemid = input.dataset.listitemid;
        input.setAttribute("data-confirm", true);
        ViewList.sendDotNetMessage("PrepConfirm", listitemid, null, input.value);
    },
    saveCurrentValue: function (input) {
        let listitemid = input.dataset.listitemid;
        if (input.dirty) {
            input.dirty = false;
            ViewList.sendDotNetMessage("SaveNewProductType", listitemid, null, input.value);
        }
        let next = ViewList.getNextTabbedInput(input.tabIndex);
        next && next.focus();
        !next && input.focus();
    },
    saveListItem: function (listitemid, producttypeid) {
        ViewList.sendDotNetMessage("AssignProductType", listitemid, producttypeid);
    },
    itemNameOnInputChange: function (e) {
        let listitemid = this.dataset.listitemid;
        this.value && ViewList.sendDotNetMessage("CheckProductTypeMatch", listitemid, null, this.value);
        ViewList.filterProductTypes(this);
        this.dirty = true;
    },
    getNextTabbedInput: function (tabIndex) {
        let nextTab = tabIndex + 1;
        let list = [...new Set(document.getElementsByClassName("list_item_name_input"))];
        return list && list.filter(elem => elem.tabIndex == nextTab)[0];
    },
    restoreOriginal: function (input) {
        if (!input) return;
        let [listitemid, producttypeid] = ViewList.getEntryAttrs(input);
        let original = input.dataset.original;
        if (original != 0 && original != producttypeid) {
            ViewList.saveListItem(listitemid, original)
        }
    },
    itemNameOnKeyUp: function (e) {
        let tabIndex = this.tabIndex;
        let fromSelector = false;
        let input = this;
        let _nextInput = function () {
            let conf = input.dataset.confirm;
            if (!input.value && !fromSelector) {
                ViewList.restoreOriginal(input);
                ViewList.filterEmpty();
            }
            let nextElem = ViewList.getNextTabbedInput(tabIndex);
            if (nextElem) {
                !fromSelector && conf != "true" && nextElem.focus();
            } else {
                !!input.value && ViewList.prepAddNewListItem();
            }
        }

        if (e.key == "Enter") {
            if (ViewList.productTypesShown) {
                ViewList.productTypeSelected(this);
                fromSelector = true;
            }
            let inConfirm = input.dataset.confirm == "true";
            if (inConfirm) {
                SaveCurrentValue(input);
            }
            _nextInput();
        }
    },
    documentOnKeyUp: function (e) {
        let input = ViewList.getCurrentInput();
        if (e.key == "ArrowUp" || e.key == "ArrowDown") {
            if (!ViewList.productTypesShown) {
                return setTimeout(() => {
                    let [listitemid, producttypeid] = ViewList.getEntryAttrs(input);
                    ViewList.clearProductFilter(listitemid)
                    ViewList.showProductTypes(input)
                    if (!ViewList.changeProductSelection(listitemid, producttypeid))
                        ViewList.highlightFirstSelectorItem(listitemid);
                }, 300);
            }
            ViewList.productTypesShown && ViewList.highlightNextItem(input, e.key == "ArrowUp");
            return;
        }
        if (e.key == "Tab") {
            let nextTab = -1;
            let list = [...new Set(document.getElementsByClassName("list_item_name_input"))];
            if (!e.shiftKey && input.tabIndex >= list.length) {
                nextTab = 1;
            }
            if (e.shiftKey && input.tabIndex == 1) {
                nextTab = list.length;
            }
            if (nextTab > -1) {
                let tElem = list && list.filter(elem => elem.tabIndex == nextTab)[0];
                tElem && setTimeout(() => tElem.focus(), 100);
            }
            return;
        }
        ViewList.keyOnDocument(e)
    },
    productTypeClicked: function () {
        ViewList.productTypeSelected(this);
    },
    productTypeSelected: function (elem) {
        let listitemid, producttypeid;
        if (!elem || !elem.classList) return;
        let selectedPt = elem.classList.contains("producttypeitem");
        if (selectedPt) {
            producttypeid = elem.dataset.producttypeid;
            let selector = elem.closest(".producttypeselector");
            listitemid = selector && selector.dataset.listitemid;
        }
        let selectedInp = elem.classList.contains("list_item_name_input");
        if (selectedInp) {
            let [_listitemid, _producttypeid] = ViewList.getEntryAttrs(elem);
            listitemid = _listitemid;
            let ps = ViewList.getPTSel(listitemid);
            let selectedElem = ps && ps.querySelector(".producttypeitem.selected");
            producttypeid = selectedElem && selectedElem.dataset.producttypeid;
        }

        if (!producttypeid || !listitemid) return;
        let input = document.querySelector(".list_item_name_input[data-listitemid='" + listitemid + "']")
        input && (input.dirty = false);
        selectedPt && input && input.focus();
        ViewList.saveListItem(listitemid, producttypeid);
        ViewList.dismissProductTypes();
    },
    isSelectorGroup: function (e) {
        return e.target.classList.contains("producttypeitem") ||
            e.target.classList.contains("producttypeselector") ||
            e.target.parentElement.classList.contains("producttypeselector") ||
            e.target.classList.contains("list_item_name_input") ||
            e.target.parentElement.classList.contains("list_item_name_input");
    },
    inputOrSelectorFocused: function (e) {
        let isSG = ViewList.isSelectorGroup(e);
        if (isSG) {
            return;
        }
        ViewList.dismissProductTypes();
    },
    keyOnDocument: function (e) {
        if (e.key == "Escape") {
            ViewList.dismissProductTypes();
        }
    },
    initListItemEditor: function () {
        document.querySelectorAll(".list_item_name_input").forEach(elem => {
            elem.removeEventListener("blur", ViewList.itemNameOnBlur);
            elem.addEventListener("blur", ViewList.itemNameOnBlur);
            elem.removeEventListener("focus", ViewList.itemNameOnFocus);
            elem.addEventListener("focus", ViewList.itemNameOnFocus);
            elem.removeEventListener("keyup", ViewList.itemNameOnKeyUp);
            elem.addEventListener("keyup", ViewList.itemNameOnKeyUp)
            elem.removeEventListener("input", ViewList.itemNameOnInputChange);
            elem.addEventListener("input", ViewList.itemNameOnInputChange)
        });

        document.querySelectorAll(".producttypeitem").forEach(elem => {
            elem.removeEventListener("click", ViewList.productTypeClicked);
            elem.addEventListener("click", ViewList.productTypeClicked);
        });

        document.querySelectorAll(".quantity_input .rz-inputtext").forEach(elem => {
            elem.removeEventListener("keyup", ViewList.resizeTextInputToFit);
            elem.addEventListener("keyup", ViewList.resizeTextInputToFit);
            elem.removeEventListener("focus", ViewList.itemNameOnFocus);
            elem.addEventListener("focus", ViewList.itemNameOnFocus);
            elem.removeEventListener("blur", ViewList.saveCurrentQuantity);
            elem.addEventListener("blur", ViewList.saveCurrentQuantity);
        });

        globalInteractions.removeDocumentKeyUpCallback("ViewList", ViewList.documentOnKeyUp);
        globalInteractions.addDocumentKeyUpCallback("ViewList", ViewList.documentOnKeyUp);
        globalInteractions.removeDocumentMouseDownCallback("ViewList", ViewList.keyOnDocument);
        globalInteractions.addDocumentMouseDownCallback("ViewList", ViewList.keyOnDocument);
    }
}

var InitViewList = ViewList.initViewList;
var SaveCurrentValue = ViewList.saveCurrentValue;
var FocusLastListItem = ViewList.focusLastListItem;
var SetDotNetRef = ViewList.setDotNetRef;
var RestoreOriginal = ViewList.restoreOriginal;
var EnableDisableTabIndices = ViewList.enableDisableTabIndices;

export {
    InitViewList,
    SaveCurrentValue,
    FocusLastListItem,    
    SetDotNetRef,
    RestoreOriginal,
    EnableDisableTabIndices
};
