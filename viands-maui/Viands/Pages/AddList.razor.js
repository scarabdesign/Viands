
var AddList = {
    dotNetRef: null,
    producSelectorOpen: false,
    setDotNetRef: function (dnr) {
        window.AddList = AddList; //temporary
        AddList.dotNetRef = dnr;
        AddList.init();
    },
    init: function () {
        globalInteractions.removeDocumentBeforeInputCallback("AddList", AddList.onBeforeInput);
        globalInteractions.addDocumentBeforeInputCallback("AddList", AddList.onBeforeInput);
        globalInteractions.removeDocumentKeyUpCallback("AddList", AddList.onKeyUp);
        globalInteractions.addDocumentKeyUpCallback("AddList", AddList.onKeyUp);
        globalInteractions.removeDocumentMouseDownCallback("AddList", AddList.onMouseDown);
        globalInteractions.addDocumentMouseDownCallback("AddList", AddList.onMouseDown);
        AddList.addEventListeners();
        AddList.adjustToolbarButtons();
    },
    addEventListeners: function () {
        let prods = document.querySelector(".inset_page");
        prods && prods.addEventListener("scroll", () => {
            if (AddList.producSelectorOpen) {
                AddList.dotNetRef && AddList.dotNetRef.invokeMethodAsync("HideProductSelection");
                AddList.producSelectorOpen = false;
            }
        });
    },
    onMouseDown: function (e) {
        if (e.target && e.target.closest(".rz-tooltip") != null) {
            return;
        }

        AddList.cleanUpEditor();

        if (e.target && e.target.closest("[contenteditable]") == null) {
            return;
        }

        let currentProd = AddList.getCurrentProductElement();
        if (currentProd == e.target) {
            return AddList.showProductElementOptions();
        }

        let currentSetTitleElem = AddList.getCurrentSetTitleElement();
        if (currentSetTitleElem != null) {

            return AddList.showSetTitleElementOptions();
        }
    },
    setNameChanged: function () {
        let currentSetTitleElem = AddList.getCurrentSetTitleElement();
        return currentSetTitleElem.hasAttribute("dirty");
    },
    countNewSetItems: function () {
        let currentSetElem = AddList.getCurrentSetElement();
        if (currentSetElem) {
            return currentSetElem.querySelectorAll(":scope > productelement[data-newtoset='true']").length;
        }
    },
    countUnimportedItems: function () {
        let currentSetElem = AddList.getCurrentSetElement();
        if (currentSetElem) {
            return currentSetElem.querySelectorAll(":scope > productelement:not([data-productid='0'],[data-liid])").length;
        }
    },
    countUnsavedItems: function () {
        let currentSetElem = AddList.getCurrentSetElement();
        if (currentSetElem) {
            return [...currentSetElem.querySelectorAll(":scope > productelement")]
                .map(e => {
                    return e.innerText.trim() &&
                        (e.hasAttribute("dirty") ||
                            (e.dataset && e.dataset.productid && e.dataset.productid == 0))
                }).filter(Boolean).length;
        }
    },
    showSetTitleElementOptions: function () {
        AddList.doCleanUpEditor();
        let currentSetTitleElem = AddList.getCurrentSetTitleElement();
        if (currentSetTitleElem) {

            let setname = currentSetTitleElem.innerText.trim();
            if (!setname) return;

            let setid = 0, X = 0, Y = 0;
            let changed = currentSetTitleElem.hasAttribute("dirty") ? 1 : 0;

            if (currentSetTitleElem.dataset) {
                setid = currentSetTitleElem.dataset.setid || 0;
            }

            let bc = currentSetTitleElem.getBoundingClientRect();
            if (!!bc) {
                X = parseInt(bc.right);
                Y = parseInt(bc.top);
            }
            
            let info = AddList.getCurrentProductInfo();
            let dirtyCount = AddList.countUnsavedItems();
            let unimportedCount = AddList.countUnimportedItems();
            let newsetitemscount = AddList.countNewSetItems();

            let message = {
                SetName: setname,
                SetId: parseInt(setid),
                HasChanged: changed,
                UnsavedProductCount: dirtyCount,
                SetItemsDirty: dirtyCount > 0 ? 1 : 0,
                HasUnimportedItems: unimportedCount > 0 ? 1 : 0,
                NewSetItemsCount: newsetitemscount,
                UnimportedItemsCount: unimportedCount,
                SetCount: info.SetCount,
                X: X, Y: Y
            };

            AddList.dotNetRef && AddList.dotNetRef.invokeMethodAsync("SetElementTitleClicked", JSON.stringify(message));
        }
    },
    showProductElementOptions: function () {
        let currentProd = AddList.getCurrentProductElement();
        let currentSetTitle = AddList.getCurrentSetTitle();
        if (currentProd) {

            let name = currentProd.innerText.trim();
            if (!name) return;

            let setname = null;
            let setid = 0, newtoset = false, productid = 0, X = 0, Y = 0;
            let changed = currentProd.hasAttribute("dirty") ? true : false;
            var shouldImport = true;
            if (currentProd.dataset && currentProd.dataset.liid) {
                shouldImport = false;
            }

            if (currentProd.dataset) {
                setid = currentProd.dataset.setid || 0;
                productid = currentProd.dataset.productid || 0;
                newtoset = currentProd.dataset.newtoset == "true" ? true : false;
            }

            if (setid == 0) {
                setid = AddList.getSetIdForProduct(currentProd);
            }

            if (setid != 0) {
                setname = currentSetTitle && currentSetTitle.innerText && currentSetTitle.innerText.trim();
            }

            let bc = currentProd.getBoundingClientRect();
            if (!!bc) {
                X = parseInt(bc.right);
                Y = parseInt(bc.top);
            }

            let message = {
                Name: name,
                ProductTypeId: parseInt(productid),
                SetName: setname,
                SetId: parseInt(setid),
                HasChanged: changed ? 1 : 0,
                NewToSet: newtoset ? 1 : 0,
                UnimportedItemsCount: shouldImport ? 1 : 0,
                X: X, Y: Y
            };

            return AddList.dotNetRef && AddList.dotNetRef.invokeMethodAsync("ProductElementClicked", JSON.stringify(message));
        }
    },
    getSetIdForProduct: function (prodElem) {
        let setid = 0;
        if (prodElem && prodElem.dataset) {
            setid = prodElem.dataset.setid || 0;
            if (setid == 0) {
                let setElem = prodElem.closest("setelement");
                setid = (setElem && setElem.dataset && setElem.dataset.setid) || 0;
            }
        }
        return setid;
    },
    onKeyUp: function (e) {
        let currentElem = AddList.getCurrentProductElement();
        if (currentElem == null) {
            currentElem = AddList.getCurrentSetTitleElement();
        }
        if (currentElem != null) {
            let elemText = currentElem.textContent.toString();
            let ogText = currentElem.dataset && currentElem.dataset.og && currentElem.dataset.og.trim();
            if (ogText != null) {
                if (elemText.trim() != ogText) {
                    currentElem.setAttribute("dirty", true);
                }
                if (!elemText.trim()) {
                    var tag = currentElem.tagName.toLowerCase();
                    if (tag == "productelement") {
                        currentElem.dataset.productid = 0;
                        delete currentElem.dataset.liid;
                        delete currentElem.dataset.og;
                    }
                }
            }
            if (currentElem.dataset && currentElem.dataset.setid == 0) {
                currentElem.dataset.og = elemText.trim();
            }
        }

        AddList.triggerEditorUpdate();
        AddList.adjustToolbarButtons();
        AddList.cursorToCurrentEnd();
        AddList.showProductSelection();
    },
    adjustUndoButton: function (hasHistory) {
        document.querySelector(".undo_button").disabled = !hasHistory;
        AddList.cleanUpEditor();
    },
    hasUnsaved: function () {
        return !!document.querySelectorAll("[dirty], [data-productid='0'], setelement[data-setid='0']").length;
    },
    adjustToolbarButtons: function () {
        let hasUnsaved = AddList.hasUnsaved();
        let saveButton = document.getElementsByClassName("save_all_products").item(0);
        let hasUnimported = document.querySelectorAll("productelement:not([data-liid])").length > 0;
        saveButton && (saveButton.disabled = !hasUnsaved);
        let importButton = document.getElementsByClassName("import_all_sets").item(0);
        if (importButton) {
            importButton.disabled = false;
            if (hasUnsaved) {
                importButton.disabled = true;
            } else if (!hasUnimported) {
                importButton.disabled = true;
            }
        }
        let menulabel = document.getElementsByClassName("menu_editor_label").item(0);
        menulabel && (hasUnsaved ? menulabel.classList.add("unsaved") : menulabel.classList.remove("unsaved"));
    },
    onBeforeInput: function (e) {
        if (e.inputType == "historyRedo") {
            return;
        }

        if (e.inputType == "historyUndo") {
            AddList.dotNetRef && AddList.dotNetRef.invokeMethodAsync("UndoChange");
            return;
        }

        if (e.target.hasAttribute("contenteditable")) {

            let keyval = null;
            if ((!!e.data && e.data == " ") || (e.code && e.code == "Space")) {
                keyval = "Space";
            }
            if ((e.inputType && e.inputType == "insertParagraph") || (e.code && e.code == "Enter") || (e.key && e.key == "Enter")) {
                keyval = "Enter";
            }
            let s = document.getSelection();
            let atEnd = s.anchorOffset == s.anchorNode.length;
            let currentElem = AddList.getCurrentProductElement();
            if (currentElem == null) {
                currentElem = AddList.getProductElement();
            }

            if (currentElem && !currentElem.parentElement) return;

            let currentSetElem = AddList.getCurrentSetElement();

            if (currentElem != null) {
                if ((keyval == "Space" && atEnd) || keyval == "Enter") {
                    if (keyval == "Enter") {
                        if (!AddList.adjustProductName(currentElem)) {
                            setTimeout(AddList.cleanUpEditor);
                            AddList.triggerEditorUpdate();
                            return;
                        }
                        e.preventDefault();
                    }

                    let elemText = currentElem.textContent.toString();
                    if (elemText.trim().length > 0) {
                        if (elemText.search(/\s$/) > -1) {
                            if (!AddList.adjustProductName(currentElem)) {
                                AddList.cursorToEnd(currentElem);
                                AddList.triggerEditorUpdate();
                                return;
                            }

                            let setid = (currentElem.dataset && currentElem.dataset.setid || 0);
                            AddList.removeEmptyElements();
                            let endElem = currentSetElem && AddList.cursorToEnd(currentSetElem);
                            let spaceChar = document.createTextNode("\u00A0");
                            endElem.after(spaceChar);
                            let newElem = AddList.replaceWithNewProduct(spaceChar, setid);
                            AddList.selectElement(newElem);
                            e.preventDefault();
                            AddList.triggerEditorUpdate();
                            return;
                        }
                    } else {
                        AddList.adjustProductName(currentElem);
                        AddList.cursorToEnd(currentElem);
                        AddList.cleanUpEditor();
                    }
                }
                AddList.triggerEditorUpdate();
                return;
            }
            if (currentElem == null) {
                currentElem = AddList.getCurrentSetTitleElement();
            }
            if (currentElem != null) {
                let elemText = currentElem.textContent.toString();
                let setid = (currentElem.dataset && currentElem.dataset.setid || 0);
                if ((keyval == "Space" && atEnd) || keyval == "Enter") {
                    if (keyval == "Enter") {
                        if (s.anchorOffset == 0) {
                            currentElem.parentElement &&
                                currentElem.parentElement.parentElement &&
                                currentElem.parentElement.parentElement.insertBefore(document.createElement("br"), currentElem.parentElement);
                            e.preventDefault();
                            AddList.cleanUpEditor();
                            AddList.triggerEditorUpdate();
                            return;
                        }
                        if (!AddList.adjustSetName()) {
                            AddList.cursorToEnd(currentElem);
                            AddList.cleanUpEditor();
                            AddList.triggerEditorUpdate();
                            return;
                        }

                        e.preventDefault();
                        AddList.cleanUpEditor();
                    }
                    if (elemText.trim().length > 0) {
                        if (elemText.search(/\s$/) > -1) {
                            if (!AddList.adjustSetName()) {
                                AddList.cursorToEnd(currentElem);
                                AddList.selectLastProductElem();
                                AddList.triggerEditorUpdate();
                                return;
                            }

                            AddList.removeEmptyElements();
                            let endElem = currentSetElem && AddList.cursorToEnd(currentSetElem);
                            let spaceChar = document.createTextNode("\u00A0");
                            endElem.after(spaceChar);
                            let newElem = AddList.replaceWithNewProduct(spaceChar, setid);
                            AddList.selectElement(newElem);
                            e.preventDefault();
                        }
                    } else {
                        AddList.adjustSetName();
                        AddList.cursorToEnd(currentElem);
                        AddList.cleanUpEditor();
                    }
                }

                AddList.triggerEditorUpdate();
                return;
            }
        }

        AddList.cleanUpEditor();
    },
    cursorToCurrentEnd: function () {
        let currentElem = AddList.getCurrentProductElement() || AddList.getCurrentSetTitleElement()
        if (currentElem &&
            currentElem.innerHTML &&
            currentElem.innerHTML.length > 2 && (
                currentElem.innerHTML.search(/ &nbsp;$/) > -1 ||
                currentElem.innerHTML.search(/&nbsp;&nbsp;$/) > -1 ||
                currentElem.innerHTML.search(/&nbsp; $/) > -1
            )
        ) {
            AddList.cursorToEnd(currentElem);
        }
    },
    showProductSelection: function (original, originalid) {
        let cprod = AddList.getCurrentProductElement();
        if (cprod) {
            let bc = cprod.getBoundingClientRect();
            if (!bc) return;
            let Y = parseInt(bc.top);
            let X = parseInt(bc.right);
            let info = AddList.getCurrentProductInfo();
            let setId = info && parseInt(info.SetId) || 0;
            let message = {
                "Name": cprod.innerText.trim() ? cprod.innerText : original,
                "HasChanged": original != null && cprod.innerText.trim() != original ? 1 : 0,
                "ProductTypeId": parseInt(originalid) || info.ProductTypeId,
                "SetId": setId,
                "NewToSet": info.NewToSet ? 1 : 0,
                "X": X,
                "Y": Y
            };
            AddList.dotNetRef && AddList.dotNetRef.invokeMethodAsync("ShowProductSelection", JSON.stringify(message));
            AddList.producSelectorOpen = true;
        } else {
            AddList.dotNetRef && AddList.dotNetRef.invokeMethodAsync("HideProductSelection");
            AddList.producSelectorOpen = false;
        }
    },
    removeCurrentProductElem: function (productid, setid) {
        let cprod = AddList.getProductElement();
        if (cprod && cprod.dataset) {
            if (productid === parseInt(cprod.dataset.productid) &&
                setid === parseInt(cprod.dataset.setid)) {
                cprod.remove();
                AddList.triggerEditorUpdate();
            }
        }
    },
    removeCurrentSetElem: function (setid) {
        let se = AddList.getSetElem();
        if (se && se.dataset && se.dataset.setid == setid) {
            se.remove();
            AddList.triggerEditorUpdate();
        }
    },
    reportToConsole: function (message) {
        let currentDate = new Date();
        let formattedDate = currentDate.toLocaleString(undefined, {
            year: 'numeric', month: 'numeric', day: 'numeric',
            hour: 'numeric', minute: 'numeric', second: 'numeric',
            fractionalSecondDigits: 3
        });
        console.log(message, formattedDate);
    },
    triggerEditorUpdate: function () {
        let editor = document.querySelector("[contenteditable]");
        editor && AddList.dotNetRef && AddList.dotNetRef.invokeMethodAsync("EditorStateHasChanged", editor.innerHTML);
        AddList.adjustToolbarButtons();
    },
    getProductIdsAndQuantitiesForCurrentSet: function () {
        let prodsAndQuants = {};
        let setElem = AddList.getCurrentSetElement();
        let ids = [...setElem.querySelectorAll("productelement:not([data-productid='0'], [data-liid])")].map(p => parseInt(p.dataset.productid));
        ids.map(id => prodsAndQuants[id] = (prodsAndQuants[id] || 0) + 1);
        return prodsAndQuants;
    },
    getProductIdsAndQuantities: function () {
        let prodsAndQuants = {};
        let editor = document.querySelector("[contenteditable]");
        let ids = [...editor.querySelectorAll("productelement:not([data-productid='0'], [data-liid])")].map(p => parseInt(p.dataset.productid));
        ids.map(id => prodsAndQuants[id] = (prodsAndQuants[id] || 0) + 1);
        return prodsAndQuants;
    },
    updateProductsWithListIds: function (listids) {
        Object.keys(listids).forEach(prid => {
            let editor = document.querySelector("[contenteditable]");
            let elems = editor.querySelectorAll("[data-productid='" + prid + "']");
            elems.forEach(e => e.dataset.liid = listids[prid]);
        });
        AddList.triggerEditorUpdate();
        setTimeout(AddList.saveMenu, 500);
    },
    saveMenu: function () {
        AddList.dotNetRef && AddList.dotNetRef.invokeMethodAsync("SaveMenu");
    },
    replaceCurrentProdutText: function (selectedName, productid, newtoset) {
        let cprod = AddList.getProductElement();
        if (cprod) {
            cprod.dataset.productid = productid;
            cprod.dataset.og = selectedName;
            if (newtoset) {
                cprod.dataset.newtoset = true;
            } else {
                delete cprod.dataset.newtoset;
            }
            delete cprod.dataset.liid;
            cprod.removeAttribute("dirty");
            cprod.innerHTML = '&nbsp;' + selectedName + '&nbsp;';
            let setelem;
            if (setelem = AddList.getSetElem()) {
                AddList.addNewProductToSet(setelem, null, true, cprod.dataset.setid || 0);
                AddList.selectLastProductElemOfSet(setelem);
            } else {
                let spaceChar = document.createTextNode("\u00A0");
                cprod.after(spaceChar);
                let newElem = AddList.replaceWithNewProduct(spaceChar, 0);
                AddList.selectElement(newElem);
            }
            AddList.triggerEditorUpdate();
        }
    },
    openSelectProducts: function () {
        AddList.showProductSelection();
    },
    cleanUpEditor: function () {
        setTimeout(AddList.doCleanUpEditor);
    },
    doCleanUpEditor: function () {
        AddList.removeEmptyElements();
        AddList.removeWrappingDivs();
        AddList.removeSpans();
        AddList.removeStyles();
        AddList.removeMutipleSpaces();
        AddList.adjustToolbarButtons();
    },
    removeMutipleSpaces: function () {
        let toomanyspacebars = String.fromCharCode(160, 32, 160, 32);
        let toomanynbsps = String.fromCharCode(160, 160);
        let spaces = String.fromCharCode(160, 32);
        let nbsps = String.fromCharCode(160);
        let rerun = false;
        let setElems = document.querySelectorAll("[contenteditable] setelement");
        if (setElems) {
            [...setElems].forEach(e => [...e.childNodes].forEach(ce => {
                if (!ce.tagName && ce.textContent.search(toomanynbsps) > -1) {
                    ce.textContent = ce.textContent.replace(toomanynbsps, nbsps);
                    rerun = true;
                }
                if (!ce.tagName && ce.textContent.search(toomanyspacebars) > -1) {
                    ce.textContent = ce.textContent.replace(toomanyspacebars, spaces);
                    rerun = true;
                }
            }))
            if (rerun) {
                AddList.removeMutipleSpaces();
            }
        }
    },
    removeSpans: function () {
        let spans = document.querySelectorAll("[contenteditable] span");
        spans && spans.forEach(s => s.outerHTML = s.innerHTML);
    },
    removeStyles: function () {
        document.querySelectorAll("[contenteditable] * ").forEach(e => e.removeAttribute("style"));
    },
    removeWrappingDivs: function () {
        let editor = document.querySelector("[contenteditable]");
        let runagain = false;
        editor && editor.childElementCount > 0 && editor.childNodes.forEach(n => {

            let tag = n && n.tagName && n.tagName.toLowerCase();
            if (tag && tag == "div" && (n.outerHTML != "<div><br></div>")) {
                if (n.childElementCount > 1) {
                    n.outerHTML = n.innerHTML;
                    runagain = true;
                }
            }
        });
        if (runagain) {
            AddList.removeWrappingDivs();
        }
    },
    prepSetAndProductsForSaving: function () {
        document.querySelectorAll("setelement").forEach(elem => {
            let title = elem.querySelector("setelementtitle");
            let prods = elem.querySelectorAll("productelement");
            let titleEmpty = !title.innerText.trim();
            let removeAll = titleEmpty && !prods.length;
            if (removeAll) {
                elem.remove();
                return;
            }
            if (titleEmpty) {
                let ogtitle = title.dataset && title.dataset.og;
                let settitle = ogtitle || "New Set";
                title.innerHTML = "&nbsp;" + settitle + "&nbsp;";
                if (ogtitle) {
                    title.removeAttribute("dirty")
                }
            }
        });
    },
    removeEmptyElements: function () {
        let rerun = false;
        let prodbrs = document.querySelectorAll("productelement br");
        prodbrs.length && prodbrs.forEach(e => {
            rerun = true;
            e.remove();
        });
        let emptyprods = document.querySelectorAll("productelement:empty");
        emptyprods.length && emptyprods.forEach(e => {
            rerun = true;
            e.remove();
        });
        let prods = document.querySelectorAll("productelement");
        let s = document.getSelection();
        prods.length && prods.forEach(e => {
            if (e == s.anchorNode || (e.firstChild && e.firstChild == s.anchorNode)) {
                return;
            }
            if (!e.innerText.trim()) {
                e.remove();
            }
        });
        let emptysettiles = document.querySelectorAll("setelementtitle:empty");
        emptysettiles.length && emptysettiles.forEach(e => {
            rerun = true;
            e.remove();
        });
        let setbrs = document.querySelectorAll("setelement br");
        setbrs.length && setbrs.forEach(e => {
            rerun = true;
            e.remove();
        });
        let emptysets = document.querySelectorAll("setelement:empty");
        emptysets.length && emptysets.forEach(e => {
            rerun = true;
            e.replaceWith(document.createElement("br"));
        });
        let emptydivs = document.querySelectorAll("[contenteditable] div:empty");
        emptydivs.length && emptydivs.forEach(e => {
            rerun = true;
            e.appendChild(document.createElement("br"));
        });
        if (rerun) {
            AddList.removeEmptyElements();
        }
    },
    currentProdElem: null,
    getCurrentProductElement: function () {
        let s = window.getSelection();
        let prdElem = s.anchorNode && ((s.anchorNode.closest && s.anchorNode.closest("productelement")) || (s.anchorNode.parentElement && s.anchorNode.parentElement.closest("productelement")));
        if (prdElem != null) {
            AddList.currentProdElem = prdElem;
        }

        return prdElem;
    },
    getProductElement: function () {
        let pe = AddList.getCurrentProductElement();
        if (pe == null && AddList.currentProdElem != null && AddList.currentProdElem.parentElement != null) {
            pe = AddList.currentProdElem;
        }
        return pe;
    },
    currentSetElement: null,
    getCurrentSetElement: function () {
        let s = window.getSelection();
        let setElem = s.anchorNode && (s.anchorNode.closest && s.anchorNode.closest("setelement") || s.anchorNode.parentElement.closest("setelement"));
        if (setElem != null) {
            AddList.currentSetElement = setElem;
        } 
        
        return setElem;
    },
    getCurrentSetTitleForSetElem: function (elem) {
        return elem && elem.querySelector(":scope > setelementtitle");
    },
    getCurrentSetTitle: function () {
        return AddList.getCurrentSetTitleForSetElem(AddList.getCurrentSetElement());
    },
    getSetElem: function () {
        AddList.currentSetElement = AddList.getCurrentSetElement();
        return AddList.currentSetElement;
    },
    getCurrentSetTitleElement: function () {
        let s = window.getSelection();
        if (!s.anchorNode)
            return;
        if (s.anchorNode.closest)
            return s.anchorNode.closest("setelementtitle");
        if (s.anchorNode.parentElement)
            return s.anchorNode.parentElement.closest("setelementtitle");
    },
    insertEmptyProduct: function (name) {
        let message = {
            "Name": name,
            "SetId": 0,
            "ProductId": 0
        };

        AddList.dotNetRef && AddList.dotNetRef.invokeMethodAsync("InsertEmptyProductType", JSON.stringify(message));
        setTimeout(AddList.triggerEditorUpdate, 500);
    },
    currentProductTypeSaved: function (productid, newtoset) {
        let prodEl = AddList.getProductElement();
        if (prodEl) {
            if (newtoset) {
                prodEl.dataset.newtoset = true;
            } else {
                delete prodEl.dataset.newtoset;
            }
            
            AddList.productTypeSaved(prodEl, productid);
            AddList.triggerEditorUpdate();
        }
    },
    productTypeSaved: function (prodEl, productid) {
        prodEl.dataset.productid = productid
        prodEl.removeAttribute("dirty");
    },
    setSaved: function (setid) {
        let se = AddList.getSetElem();
        se && AddList.doSetSaved(se, setid);
    },
    doSetSaved: function (setElem, setid, setname) {
        setElem.dataset.setid = setid;
        let titleElem = setElem.querySelector(":scope > setelementtitle");
        titleElem.dataset.setid = setid;
        if (setname != null) {
            titleElem.dataset.og = setname;
        }
        titleElem.removeAttribute("dirty");
    },
    saveAllSetsAndProducts: async function () {

        let _processProduct = async function (prodEl, setId, saveToSet) {

            setId != 0 && (prodEl.dataset.setid = setId);
            let prodname = prodEl.innerText.trim();
            let productId = parseInt(prodEl.dataset.productid);

            //don't save empty products
            if (!prodname) return;

            let saveProduct = false;
            if (productId == 0 || prodEl.hasAttribute("dirty")) {
                saveProduct = true;
            }

            if (saveProduct) {
                //create new or save product, update element with id
                productId = await AddList.dotNetRef.invokeMethodAsync("SaveProduct", JSON.stringify({
                    "Name": prodname,
                    "ProductTypeId": productId
                }));
                prodEl.dataset.productid = productId;
                prodEl.dataset.og = prodname;
                prodEl.removeAttribute("dirty");
            }

            if (saveToSet && !!setId ) {
                //if new set, add product to set
                var success = await AddList.dotNetRef.invokeMethodAsync("AddProductToSet", JSON.stringify({
                    "SetId": setId,
                    "ProductTypeId": productId
                }));
                if (success) {
                    delete prodEl.dataset.newtoset;
                    AddList.productTypeSaved(prodEl, productId);
                }
            }
        }

        //Clean up empty sets and setnames
        AddList.prepSetAndProductsForSaving();

        // - JS gets all first level relevant elements
        let firstClassObjects = [...document.querySelectorAll("setelement, [contenteditable] > productelement")];
        if (!firstClassObjects.length) {
            return;
        }

        // - loop through them
        await Promise.all(firstClassObjects.map(async (elem, index)=> {
            let tagName = elem.tagName.toLowerCase();
            if (tagName == "setelement") {
                
                let setId = elem.dataset && parseInt(elem.dataset.setid) || 0;
                let titleElem = elem.querySelector("setelementtitle");
                let setName = titleElem.innerText.trim();
                //don't save nameless sets
                if (!setName) return;
                // - note new sets for later
                let setIsNew = setId == 0;
                // - if new set or dirty
                if (setIsNew || titleElem && titleElem.hasAttribute("dirty")) {
                    //send, get new setid
                    setId = await AddList.dotNetRef.invokeMethodAsync("SaveSet", JSON.stringify({
                        "SetName": setName,
                        "SetId": setId
                    }))
                    if (setId == 0) return;//somehow failed
                    
                    AddList.doSetSaved(elem, setId, setName);
                }

                // - gather product info from current set where dirty
                let products = [...elem.querySelectorAll("productelement")];
                if (products.length) {
                    await Promise.all(products.map(async (elem) => await _processProduct(elem, setId, setIsNew)));
                }

            } else if (tagName == "productelement") {
                _processProduct(elem);
            }
        }));

        AddList.triggerEditorUpdate();
        setTimeout(AddList.saveMenu, 500);
    },
    productTypesSaved: function (setid, productids, addedToSet) {
        AddList.setSaved(setid);
        let se = AddList.getSetElem();
        if (se && productids.length) {
            let itemsElems = se.querySelectorAll(":scope > productelement");
            itemsElems.forEach(prodEl => {
                if (prodEl.dataset.productid == 0) {
                    let prodname = prodEl.innerText.trim();
                    let prodId = productids.filter(p => p.productTypeName == prodname)[0];
                    prodEl.dataset.productid = (prodId && prodId.productTypeId) || 0;
                    prodEl.innerHTML = "&nbsp;" + prodname + "&nbsp;";
                }
                prodEl.dataset.setid = setid;
                prodEl.removeAttribute("dirty");
                if (addedToSet) {
                    delete prodEl.dataset.newtoset;
                }
            });
            AddList.triggerEditorUpdate();
        }
    },
    undoChangesToSetTitle: function (setid) {
        let setEl = AddList.getSetElem();
        let titleElem = setEl && setEl.querySelector(":scope > setelementtitle");
        if (titleElem && titleElem.dataset && titleElem.dataset.og && titleElem.dataset.setid == setid) {
            titleElem.innerHTML = "&nbsp;" + titleElem.dataset.og + "&nbsp;";
            titleElem.removeAttribute("dirty");
            AddList.triggerEditorUpdate();
        }
    },
    undoChangesToProductType: function (productid) {
        let prodEl = AddList.getProductElement();
        if (prodEl && prodEl.dataset && prodEl.dataset.og && prodEl.dataset.productid == productid) {
            prodEl.innerHTML = "&nbsp;" + prodEl.dataset.og + "&nbsp;";
            prodEl.removeAttribute("dirty");
            AddList.triggerEditorUpdate();
        }
    },
    replaceProduct: function () {
        let prodEl = AddList.getProductElement();
        if (prodEl) {
            prodEl.innerHTML = "&nbsp;";
            let ogid = prodEl.dataset.productid;
            let ogname = prodEl.dataset.og;
            prodEl.dataset.productid = 0; 
            var setElem = AddList.getSetElem();
            if (!!setElem) {
                prodEl.dataset.newtoset = true;
            } else {
                delete prodEl.dataset.newtoset;
            }
            delete prodEl.dataset.liid;
            prodEl.setAttribute("dirty", true);
            AddList.selectElement(prodEl);
            AddList.showProductSelection(ogname, ogid);
        }
    },
    addEmptyProductToSet: function (setid) {
        let setElem = AddList.getSetElem();
        if (setElem && setid != null) {
            if (setid != (setElem.dataset && setElem.dataset.setid) || 0) {
                return;
            }
        }

        if (setElem) {
            AddList.selectLastProductElem();
            AddList.addNewProduct(null, true);
            AddList.selectLastProductElem();
        }
    },
    replaceSelectionWithNewProduct: function (setid) {
        let s = window.getSelection();
        return AddList.replaceWithNewProduct(s.anchorNode, setid, s.toString());
    },
    replaceWithNewProduct: function (elem, setid, name) {
        let setElem = AddList.getCurrentSetElement();
        let newProd = AddList.addNewProductToSet(setElem, name, false, setid);
        elem.replaceWith(newProd);
        return newProd;
    },
    addNewProduct: function (name, addToEnd = true) {
        let setElem = AddList.getCurrentSetElement();

        if (!setElem) {
            return AddList.insertEmptyProduct(name);
        }

        let newPrd = AddList.addNewProductToSet(setElem, name, addToEnd);
        return newPrd;
    },
    addNewProductToSet: function (setElem, name, addToEnd = true, setid = 0) {
        let emptyProduct = document.createElement("productelement");
        emptyProduct.dataset.productid = 0;
        if (setElem != null) {
            emptyProduct.dataset.newtoset = true;
        }
        emptyProduct.dataset.setid = (setElem && setElem.dataset && setElem.dataset.setid) || setid;
        emptyProduct.append(document.createTextNode("\u00A0"));
        if (name) {
            emptyProduct.append(document.createTextNode(name));
            emptyProduct.append(document.createTextNode("\u00A0"));
        }

        if (addToEnd && setElem) {
            setElem.append(emptyProduct);
        }

        return emptyProduct;
    },
    selectElement: function (elem) {
        if (!elem) return;
        let s = window.getSelection();
        let r = document.createRange();
        r.setStart(elem, 1)
        r.collapse(true)
        s.removeAllRanges()
        s.addRange(r);
    },
    selectLastProductElem: function () {
        let se = AddList.getCurrentSetElement();
        if (!se) return;
        AddList.selectLastProductElemOfSet(se);
    },
    selectLastProductElemOfSet: function (se) {
        let s = window.getSelection();
        let r = document.createRange();
        r.setStart(se.childNodes[se.childNodes.length - 1], 1)
        r.collapse(true)
        s.removeAllRanges()
        s.addRange(r);
    },
    selectFirstEmptySet: function (setid = 0) {
        let emptyTitle = document.querySelector("setelementtitle[data-setid='0']");
        if (emptyTitle && setid == 0) {
            return AddList.selectElement(emptyTitle);
        }
        AddList.selectElement(
            document.querySelector("productelement[data-setid=\"" + setid + "\"][data-productid=\"0\"]")
        );
    },
    selectNewSetName: function () {
        setTimeout(() => {
            let ns = document.querySelector("setelement[data-isnew='true']");
            if (ns == null) return;
            let r = document.createRange()
            r.setStart(ns, 0);
            r.setEnd(ns, 1);
            let s = window.getSelection();
            s.removeAllRanges()
            s.addRange(r);
            delete ns.dataset.isnew;
        }, 100);  
    },
    cursorToBottom: function () {
        let editor = document.querySelector("[contenteditable]");
        let br = document.createElement("br");
        editor.appendChild(br);
        editor && AddList.cursorToEnd(br);
    },
    cursorToEnd: function (elem) {
        if (!elem || !elem.parentElement) return;
        let r = document.createRange()
        let tEl = elem.lastChild || elem.nextSibling || elem;
        r.setStart(tEl, 0);
        r.setEnd(tEl, tEl.length);
        r.collapse(false);
        let s = window.getSelection();
        s.removeAllRanges()
        s.addRange(r);
        return tEl;
    },
    adjustSetName: function () {
        let titleEl = AddList.getCurrentSetTitleElement();
        let setName;
        if (titleEl) {
            if (setName = titleEl.textContent.toString().trim()) {
                titleEl.innerHTML = '&nbsp;' + setName + '&nbsp;';
                return setName;
            }
            titleEl.innerHTML = "";

        }
    },
    adjustProductName: function (currentElem) {
        let prodEl = null;
        if (currentElem != null && currentElem.tagName.toLowerCase() == "productelement") {
            prodEl = currentElem;
        } else {
            prodEl = AddList.getCurrentProductElement();
        }
        if (prodEl) {
            let prodName;
            if (prodName = prodEl.textContent.toString().trim()) {
                prodEl.innerHTML = '&nbsp;' + prodName + '&nbsp;';
                return prodName;
            }
            prodEl && (prodEl.innerHTML = "");
        }
    },
    getSelectedText: function () {
        return window.getSelection().toString();
    },
    getCurrentProductInfo: function () {
        let pe = AddList.getCurrentProductElement();
        let pt = AddList.getCurrentSetTitleElement();
        let si = AddList.getCurrentSetInfo();

        let info = AddList.getProductInfo(pe);

        if (si) {
            if ((!info.SetId) && si.SetId > 0) {
                info.SetId = si.SetId;
            }
            if (si.SetName && si.SetName.trim()) {
                info.SetName = si.SetName;
            }
            info.SetCount = si.SetCount;
        }

        info.CurrentTarget = (pe && pe.tagName.toLowerCase()) || (pt && pt.tagName.toLowerCase());

        return info;
    },
    getSetInfo: function (setElem) {
        let info = {};
        if (setElem.dataset.setid) {
            info.SetId = parseInt(setElem.dataset.setid);
        }
        info.SetCount = setElem.querySelectorAll(":scope > productelement").length;
        let title = setElem.querySelector(":scope > setelementtitle");
        if (title) {
            info.SetName = title.innerText.trim();
            info.SetNameChanged = title.hasAttribute("dirty") ? 1 : 0;
        }
        return info;
    },
    getCurrentSetInfo: function () {
        let info = {};
        let se = AddList.getSetElem();
        if (se && se.dataset) {
            if (se.dataset.setid) {
                info.SetId = parseInt(se.dataset.setid);
            }
            info.SetCount = se.querySelectorAll(":scope > productelement").length;
            let title = se.querySelector(":scope > setelementtitle");
            if (title) {
                info.SetName = title.innerText.trim();
            }
        }

        return info;
    },
    getAllSetAndItemInfo: function () {
        let mainItems = document.querySelectorAll("setelement, [contenteditable] > productelement");
        let returnList = [];
        mainItems.forEach(i => {
            let itemTag = i.tagName.toLowerCase();
            switch (itemTag) {
                case "setelement":
                    let setInfo = AddList.getSetInfo(i);
                    setInfo.CurrentTarget = "setelement";
                    setInfo.Products = AddList.getSetListItems(i);
                    setInfo.SetItemsDirty = setInfo.Products.map(si => si.HasChanged).filter(Boolean).length > 0 ? 1 : 0;
                    returnList.push(setInfo);
                    break;
                case "productelement":
                    let prodInfo = AddList.getProductInfo(i);
                    prodInfo.CurrentTarget = "productelement";
                    returnList.push(prodInfo);
                    break;
            }
        });

        return returnList;
    },
    getProductInfo: function (productElem) {
        let info = {};
        if (productElem && productElem.dataset) {
            if (productElem.dataset.productid) {
                info.ProductTypeId = parseInt(productElem.dataset.productid) || 0;
            }
            if (productElem.dataset.setid) {
                info.SetId = parseInt(productElem.dataset.setid) || 0;
            }
            if (productElem.dataset.newtoset) {
                info.NewToSet = productElem.dataset.newtoset == "true" ? 1 : 0;
            }
            if (productElem.innerText && productElem.innerText.trim()) {
                info.Name = productElem.innerText.trim();
            }
            if (productElem.hasAttribute("dirty")) {
                info.HasChanged = 1;
            }
        }
        return info;
    },
    getSetListItems: function (setElem) {
        let items = [];
        let itemsElems = setElem.querySelectorAll(":scope > productelement");
        itemsElems.forEach(pe => {
            let info = AddList.getProductInfo(pe);
            if (Object.keys(info).length > 0) {
                items.push(info);
            }
        });

        return items;
    },
    getCurrentSetItems: function (setid) {
        let se = AddList.getSetElem();
        if (se && se.dataset && se.dataset.setid == setid) {
            return AddList.getSetListItems(se);
        }
    }
}

var SetDotNetRef = AddList.setDotNetRef;
var GetSelectedText = AddList.getSelectedText;
var SelectNewSetName = AddList.selectNewSetName;
var ReplaceCurrentProdutText = AddList.replaceCurrentProdutText;
var OpenSelectProducts = AddList.openSelectProducts;
var RemoveCurrentProductElem = AddList.removeCurrentProductElem;
var GetCurrentProductInfo = AddList.getCurrentProductInfo;
var GetCurrentSetInfo = AddList.getCurrentSetInfo;
var ReplaceSelectionWithNewProduct = AddList.replaceSelectionWithNewProduct;
var RemoveCurrentSetElem = AddList.removeCurrentSetElem;
var ReplaceProduct = AddList.replaceProduct;
var CurrentProductTypeSaved = AddList.currentProductTypeSaved;
var ProductTypesSaved = AddList.productTypesSaved;
var SetSaved = AddList.setSaved;
var SaveAllSetsAndProducts = AddList.saveAllSetsAndProducts;
var UndoChangesToProductType = AddList.undoChangesToProductType;
var UndoChangesToSetTitle = AddList.undoChangesToSetTitle;
var SelectFirstEmptySet = AddList.selectFirstEmptySet;
var AddEmptyProductToSet = AddList.addEmptyProductToSet;
var CursorToBottom = AddList.cursorToBottom;
var GetCurrentSetItems = AddList.getCurrentSetItems;
var GetAllSetAndItemInfo = AddList.getAllSetAndItemInfo;
var GetProductIdsAndQuantitiesForCurrentSet = AddList.getProductIdsAndQuantitiesForCurrentSet;
var GetProductIdsAndQuantities = AddList.getProductIdsAndQuantities;
var UpdateProductsWithListIds = AddList.updateProductsWithListIds;
var TriggerEditorUpdate = AddList.triggerEditorUpdate;
var CleanUpEditor = AddList.cleanUpEditor;
var AdjustUndoButton = AddList.adjustUndoButton;

export {
    SetDotNetRef, GetSelectedText, UndoChangesToSetTitle,
    SelectNewSetName, ReplaceCurrentProdutText,
    OpenSelectProducts, RemoveCurrentProductElem,
    GetCurrentProductInfo, GetCurrentSetInfo, SetSaved,
    ReplaceSelectionWithNewProduct, RemoveCurrentSetElem,
    ReplaceProduct, CurrentProductTypeSaved, ProductTypesSaved,
    UndoChangesToProductType, SelectFirstEmptySet,
    AddEmptyProductToSet, CursorToBottom, GetCurrentSetItems,
    GetAllSetAndItemInfo, SaveAllSetsAndProducts,
    GetProductIdsAndQuantities, UpdateProductsWithListIds,
    GetProductIdsAndQuantitiesForCurrentSet,
    TriggerEditorUpdate, CleanUpEditor, AdjustUndoButton
}