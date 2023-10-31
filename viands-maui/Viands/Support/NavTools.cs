using Blazicons;
using System.Diagnostics;

namespace Viands.Support
{
    public static class NavTools
    {
        public enum ToolTypes
        {
            Test,
            Settings,
            Home,
            CreateList,
            EditLists,
            FinishEditLists,
            EditListItems,
            FinishEditListItems,
            CancelEdit,
            SaveEdit,
            SaveAndAdd,
            AddListItem,
            ManageData,
            AddProduct,
            ScanBarcode,
            AddLocation,
            SaveLocation,
            AddTemplate,
            Templates,
            EditTemplates,
            ProductSets,
            AddProductSet,
            EditProductSets,
            ViewProductSet,
            EditProductSetItems,
            AddProductSetItem,
            SelectProductSets,
            ShowButtonTitles,
            EditorNextButton,
            EditorPrevButton,
            EditorCloseButton,
            FinishEditProduct
        }

        public enum ToolSetTypes
        {
            ListsHome,
            EditLists,
            EditList,
            ViewList,
            EditItems,
            ManageData,
            EditProducts,
            EditLocations,
            Templates,
            ViewTemplate,
            EditTemplates,
            EditTemplateItems,
            Loading,
            ProductSets,
            EditProductSets,
            AddProductSet,
            ViewProductSet,
            EditProductSetItems,
            AddProductSetItem
        }

        public enum VPageTypes
        {
            Home,
            ViewList,
            EditList, 
            ManageData,
            EditProducts,
            EditLocations,
            Templates, 
            ViewTemplate,
            EditTemplateItems,
            Loading,
            ProductSets,
            AddProductSet,
            EditProductSets,
            ViewProductSet,
            EditProductSetItems,
            AddProductSetItem
        }

        public class NavTool
        {
            public ToolTypes tool { get; set; }
            public string tip { get; set; }
            public SvgIcon icon { get; set; }
            public Action<dynamic> onclick { get; set; }
            public Func<dynamic?, Task> onclickAsync { get; set; }
            public Action onholdDown { get; set; }
            public Action onholdUp { get; set; }
            public string href { get; set; }
            public bool right { get; set; }
            public bool hidden { get; set; }
            public bool always { get; set; }
            public string highlight { get; set; }
            public bool showback { get; set; }
            public bool hasTimer { get; set; }
            public int timeOut { get; set; } = 5000;
            public bool disabled { get; set; } = false;
            public bool small { get; set; }
            public bool editorButton { get; set; } = false;
            public bool suppressTitle { get; set; } = false;
            public int[] titleArrows { get; set; } = new int[4];
            public string className { get; set; }
        }

        public static Dictionary<ToolSetTypes, ToolTypes[]> ToolSets = new Dictionary<ToolSetTypes, ToolTypes[]>()
        {
            { ToolSetTypes.ListsHome, new ToolTypes[] { ToolTypes.CreateList, ToolTypes.EditLists, ToolTypes.ManageData, ToolTypes.Settings } },
            { ToolSetTypes.EditLists, new ToolTypes[] { ToolTypes.FinishEditLists } },
            { ToolSetTypes.ViewList, new ToolTypes[] { ToolTypes.Home, ToolTypes.EditListItems, ToolTypes.AddListItem, ToolTypes.SelectProductSets } },
            { ToolSetTypes.EditItems, new ToolTypes[] { ToolTypes.FinishEditListItems, ToolTypes.AddListItem } },
            { ToolSetTypes.EditList, new ToolTypes[] { ToolTypes.CancelEdit, ToolTypes.SaveEdit, ToolTypes.SaveAndAdd } },
            { ToolSetTypes.ManageData, new ToolTypes[] { ToolTypes.Home } },
            { ToolSetTypes.EditProducts, new ToolTypes[] { ToolTypes.ManageData, ToolTypes.AddProduct, ToolTypes.ScanBarcode } },
            { ToolSetTypes.EditLocations, new ToolTypes[] { ToolTypes.ManageData, ToolTypes.AddLocation } },
            { ToolSetTypes.Templates, new ToolTypes[] { ToolTypes.ManageData, ToolTypes.EditTemplates, ToolTypes.AddTemplate } },
            { ToolSetTypes.EditTemplates, new ToolTypes[] { ToolTypes.FinishEditLists } },
            { ToolSetTypes.ViewTemplate, new ToolTypes[] { ToolTypes.Templates, ToolTypes.EditListItems, ToolTypes.AddListItem } },
            { ToolSetTypes.EditTemplateItems, new ToolTypes[] { ToolTypes.FinishEditListItems, ToolTypes.AddListItem } },
            { ToolSetTypes.ProductSets, new ToolTypes[] { ToolTypes.ManageData, ToolTypes.EditProductSets, ToolTypes.AddProductSet } },
            { ToolSetTypes.EditProductSets, new ToolTypes[] { ToolTypes.FinishEditLists } },
            { ToolSetTypes.AddProductSet, new ToolTypes[] { ToolTypes.CancelEdit, ToolTypes.SaveEdit, ToolTypes.SaveAndAdd } },
            { ToolSetTypes.ViewProductSet, new ToolTypes[] { ToolTypes.ProductSets, ToolTypes.EditProductSetItems, ToolTypes.AddProductSetItem } },
            { ToolSetTypes.EditProductSetItems, new ToolTypes[] { ToolTypes.FinishEditListItems, ToolTypes.AddProductSetItem } }
        };

        public static VPageTypes CurrentPage { get; set; } = VPageTypes.Home;
        public static Dictionary<VPageTypes, Tuple<string, ToolSetTypes>> VPages = new Dictionary<VPageTypes, Tuple<string, ToolSetTypes>>()
        {
            { VPageTypes.Home, new Tuple<string, ToolSetTypes>( "/", ToolSetTypes.ListsHome ) },
            { VPageTypes.ViewList, new Tuple<string, ToolSetTypes>( "/viewlist", ToolSetTypes.ViewList ) },
            { VPageTypes.EditList, new Tuple<string, ToolSetTypes>( "/addlist", ToolSetTypes.EditList ) },
            { VPageTypes.ManageData, new Tuple<string, ToolSetTypes>( "/managedata", ToolSetTypes.ManageData ) },
            { VPageTypes.EditProducts, new Tuple<string, ToolSetTypes>( "/editproducts", ToolSetTypes.EditProducts ) },
            { VPageTypes.EditLocations, new Tuple<string, ToolSetTypes>( "/editlocations", ToolSetTypes.EditLocations ) },
            { VPageTypes.Templates, new Tuple<string, ToolSetTypes>( "/templates", ToolSetTypes.Templates ) },
            { VPageTypes.ViewTemplate, new Tuple<string, ToolSetTypes>( "/viewlist", ToolSetTypes.ViewTemplate ) },
            { VPageTypes.EditTemplateItems, new Tuple<string, ToolSetTypes>( "/viewlist", ToolSetTypes.EditList ) },
            { VPageTypes.Loading, new Tuple<string, ToolSetTypes>( "/loading", ToolSetTypes.Loading ) },
            { VPageTypes.ProductSets, new Tuple<string, ToolSetTypes>( "/productsets", ToolSetTypes.ProductSets ) },
            { VPageTypes.AddProductSet, new Tuple<string, ToolSetTypes>( "/addlist", ToolSetTypes.AddProductSet ) },
            { VPageTypes.ViewProductSet, new Tuple<string, ToolSetTypes>( "/viewlist", ToolSetTypes.ViewProductSet ) }
        };

        public static void AdjustToolset(ToolSetTypes setKey)
        {
            var ts = ToolSets[setKey];
            if (ts == null) return;
            NavigationTools.ForEach(tool =>
            {
                tool.hidden = !tool.always && !ts.Contains(tool.tool);
                tool.showback = false;
            });
        }

        public static List<NavTool> Tools = new List<NavTool>();

        public static List<NavTool> NavigationTools => Tools.Where(t => t.editorButton == false).ToList();
        public static List<NavTool> EditorButtons => Tools.Where(t => t.editorButton == true).ToList();

        public static void HighlightTool(ToolTypes type, string highlight = "highlight_green")
        {
            Tools.FirstOrDefault(t =>  t.tool == type).highlight = highlight;
        }

        public static void ShowHideBackArrow(ToolTypes type, bool showHide)
        {
            Tools.FirstOrDefault(t => t.tool == type).showback = showHide;
        }

        public static NavTool GetNavTool(ToolTypes navToolType)
        {
            return NavigationTools?.FirstOrDefault(t => t.tool == navToolType);
        }

        public static NavTool GetShowTitlesNavTool()
        {
            return new NavTool()
            {
                tool = ToolTypes.ShowButtonTitles,
                tip = "Display Button Titles",
                icon = GoogleMaterialFilledIcon.QuestionMark,
                onholdDown = () => DisplayUtils.ShowNavButtonTitles = true,
                onholdUp = () => DisplayUtils.ShowNavButtonTitles = false,
                right = true,
                small = true,
                always = true,
            };
        }

        public static NavTool GetEditorTool(ToolTypes navToolType, Action<dynamic?> onClick = null, bool disabled = false, string className = null)
        {
            var tool = EditorButtons?.FirstOrDefault(t => t.tool == navToolType);
            if (tool != null)
            {
                tool.onclick = onClick;
                tool.className = className;
                tool.disabled = disabled;
            }
            return tool;
        }

        public static void RunToolSetFor(ToolSetTypes toolSetType)
        {
            switch (toolSetType)
            {
                case ToolSetTypes.ListsHome:
                    AdjustToolset(DisplayUtils.InListEditMode ? ToolSetTypes.EditLists : ToolSetTypes.ListsHome);
                    break;
                case ToolSetTypes.ViewList:
                    AdjustToolset(ToolSetTypes.ViewList);
                    ShowHideBackArrow(ToolTypes.Home, true);
                    break;
                case ToolSetTypes.EditLists:
                    AdjustToolset(DisplayUtils.InListEditMode ? ToolSetTypes.EditLists : ToolSetTypes.ListsHome);
                    HighlightTool(ToolTypes.FinishEditLists, DisplayUtils.InListEditMode ? "highlight_green" : null);
                    break;
                case ToolSetTypes.EditItems:
                    AdjustToolset(DisplayUtils.InItemEditMode ? ToolSetTypes.EditItems : ToolSetTypes.ViewList);
                    HighlightTool(ToolTypes.FinishEditListItems, DisplayUtils.InItemEditMode ? "highlight_green" : null);
                    ShowHideBackArrow(ToolTypes.Home, !DisplayUtils.InItemEditMode);
                    break;
                case ToolSetTypes.EditList:
                    DisplayUtils.ExitEditModes();
                    AdjustToolset(ToolSetTypes.EditList);
                    HighlightTool(ToolTypes.SaveEdit, "highlight_green");
                    HighlightTool(ToolTypes.SaveAndAdd, "highlight_green");
                    break;
                case ToolSetTypes.ManageData:
                    DisplayUtils.ExitEditModes();
                    AdjustToolset(ToolSetTypes.ManageData);
                    ShowHideBackArrow(ToolTypes.Home, true);
                    break;
                case ToolSetTypes.EditProducts:
                    AdjustToolset(ToolSetTypes.EditProducts);
                    HighlightTool(ToolTypes.AddProduct, "highlight_green");
                    ShowHideBackArrow(ToolTypes.ManageData, true);
                    break;
                case ToolSetTypes.EditLocations:
                    AdjustToolset(ToolSetTypes.EditLocations);
                    HighlightTool(ToolTypes.AddLocation, "highlight_green");
                    ShowHideBackArrow(ToolTypes.ManageData, true);
                    break;
                case ToolSetTypes.Templates:
                    AdjustToolset(ToolSetTypes.Templates);
                    ShowHideBackArrow(ToolTypes.ManageData, true);
                    break;
                case ToolSetTypes.EditTemplates:
                    AdjustToolset(DisplayUtils.InListEditMode ? ToolSetTypes.EditTemplates : ToolSetTypes.Templates);
                    HighlightTool(ToolTypes.FinishEditLists, DisplayUtils.InListEditMode ? "highlight_green" : null);
                    break;
                case ToolSetTypes.ViewTemplate:
                    AdjustToolset(DisplayUtils.InItemEditMode ? ToolSetTypes.EditTemplateItems : ToolSetTypes.ViewTemplate);
                    ShowHideBackArrow(ToolTypes.Templates, true);
                    break;
                case ToolSetTypes.EditTemplateItems:
                    AdjustToolset(DisplayUtils.InItemEditMode ? ToolSetTypes.EditTemplateItems : ToolSetTypes.ViewTemplate);
                    HighlightTool(ToolTypes.FinishEditListItems, DisplayUtils.InItemEditMode ? "highlight_green" : null);
                    ShowHideBackArrow(ToolTypes.Templates, !DisplayUtils.InItemEditMode);
                    break;
                case ToolSetTypes.ProductSets:
                    AdjustToolset(ToolSetTypes.ProductSets);
                    ShowHideBackArrow(ToolTypes.ManageData, true);
                    break;
                case ToolSetTypes.EditProductSets:
                    AdjustToolset(ToolSetTypes.EditProductSets);
                    HighlightTool(ToolTypes.FinishEditLists, DisplayUtils.InItemEditMode ? "highlight_green" : null);
                    break;
                case ToolSetTypes.AddProductSet:
                    AdjustToolset(ToolSetTypes.AddProductSet);
                    HighlightTool(ToolTypes.SaveEdit, "highlight_green");
                    HighlightTool(ToolTypes.SaveAndAdd, "highlight_green");
                    break;
                case ToolSetTypes.ViewProductSet:
                    AdjustToolset(ToolSetTypes.ViewProductSet);
                    ShowHideBackArrow(ToolTypes.ProductSets, true);
                    break;
                case ToolSetTypes.EditProductSetItems:
                    AdjustToolset(DisplayUtils.InItemEditMode ? ToolSetTypes.EditProductSetItems : ToolSetTypes.ViewProductSet);
                    if (DisplayUtils.InItemEditMode)
                    {
                        HighlightTool(ToolTypes.FinishEditListItems, "highlight_green");
                    }
                    else
                    {
                        ShowHideBackArrow(NavTools.ToolTypes.ProductSets, true);
                    }
                    break;
                    
            }
            GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.RefreshState, null);
        }

        public static void NavigateTo(VPageTypes pageType, string stringArg = null, int intArg = 0)
        {
            if (!VPages.ContainsKey(pageType)) return;
            CurrentPage = pageType;
            var toolType = VPages[pageType]?.Item2;
            if (toolType != null)
            {
                RunToolSetFor(toolType ?? ToolSetTypes.ListsHome);
            }
            GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.Navigate, new GlobalCallbacks.GeneralResponse
            {
                ResponseType = GlobalCallbacks.ResponseTypes.Navigate,
                UrlArg = VPages[pageType]?.Item1,
                StringArg = stringArg,
                IntArg = intArg
            });
        }

        public static void InitTools()
        {
            Tools.Add(new NavTool()
            {
                tool = ToolTypes.Home,
                tip = "Back to Lists",
                icon = MdiIcon.TextBoxOutline,
                onclick = (e) => NavigateTo(VPageTypes.Home),
                hidden = true
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.Templates,
                tip = "Back to Temlates",
                icon = GoogleMaterialOutlinedIcon.Checklist,
                onclick = (e) => NavigateTo(VPageTypes.Templates),
                hidden = true
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.ProductSets,
                tip = "Back to Sets",
                icon = MdiIcon.FormatListGroup,
                onclick = (e) => NavigateTo(VPageTypes.ProductSets),
                hidden = true
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.CreateList,
                tip = "Create List",
                icon = MdiIcon.TextBoxPlusOutline,
                onclick = (e) => NavigateTo(VPageTypes.EditList),
                hidden = true

            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.EditLists,
                tip = "Edit Lists",
                icon = MdiIcon.TextBoxEditOutline,
                onclick = (e) => DisplayUtils.ToggleEditListMode(),
                hidden = true
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.ManageData,
                tip = "Manage Data",
                icon = MdiIcon.DatabaseCog,
                onclick = (e) => NavigateTo(VPageTypes.ManageData),
                hidden = true
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.FinishEditLists,
                tip = "Finish Editing",
                icon = MdiIcon.CheckboxMarkedCircleOutline,
                onclick = (e) => DisplayUtils.ToggleEditListMode(),
                hidden = true
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.EditListItems,
                tip = "Edit List Items",
                icon = MdiIcon.PlaylistEdit,
                onclick = (e) => DisplayUtils.ToggleEditItemsMode(),
                hidden = true
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.FinishEditListItems,
                tip = "Finish Editing",
                icon = MdiIcon.CheckboxMarkedCircleOutline,
                onclick = (e) => DisplayUtils.ToggleEditItemsMode(),
                hidden = true
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.AddListItem,
                tip = "Add Item",
                icon = MdiIcon.PlaylistPlus,
                onclick = (e) => GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.AddListItem, true),
                hidden = true
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.SelectProductSets,
                tip = "Select Product Set",
                icon = MdiIcon.FormatListGroup,
                onclick = (e) => {
                    GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.SelectProductSets, null);
                },
                hidden = true
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.SaveAndAdd,
                tip = "Save & Add Items",
                icon = MdiIcon.CheckboxMarkedCirclePlusOutline,
                onclick = (e) => GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.SaveAndAdd, true),
                hidden = true
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.SaveEdit,
                tip = "Save Edits",
                icon = MdiIcon.CheckboxMarkedCircleOutline,
                onclick = (e) => GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.SaveEdit, true),
                hidden = true
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.CancelEdit,
                tip = "Cancel Edits",
                icon = MdiIcon.Cancel,
                onclick = (e) => GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.CancelEdit, true),
                hidden = true
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.Test,
                tip = "Test button",
                icon = MdiIcon.TestTube,
                href = "/test",
                hidden = true
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.AddProduct,
                tip = "Add Product Type",
                icon = MdiIcon.PlusCircleOutline,
                onclick = (e) => {
                    GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.EditProductType, new GlobalCallbacks.EditProductTypeResponse
                    {
                        ProductTypeId = 0,
                        IsFirst = true,
                        IsLast = false
                    });
                },
                hidden = true
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.ScanBarcode,
                tip = "Scan Barcode",
                icon = MdiIcon.Barcode,
                hasTimer = true,
                timeOut = 3000,
                onclick = (e) => {
                    GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.EditProductTypeByUPC, new GlobalCallbacks.EditProductTypeResponse
                    {
                        ProductTypeId = 0,
                        ListId = 0,
                        ListItemId = 0,
                    });
                },
                hidden = true
            });


            Tools.Add(new NavTool()
            {
                tool = ToolTypes.AddLocation,
                tip = "Add Location",
                icon = MdiIcon.PlusCircleOutline,
                onclick = (e) => {
                    GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.EditLocation, new GlobalCallbacks.EditLocationResponse
                    {
                        LocationId = 0,
                        IsFirst = true,
                        IsLast = false,
                        AddEdit = true
                    });
                },
                hidden = true
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.AddTemplate,
                tip = "Add Template",
                icon = MdiIcon.PlusCircleOutline,
                onclick = (e) => {
                    GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.EditTemplate, 0);
                },
                hidden = true
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.EditTemplates,
                tip = "Edit Templates",
                icon = GoogleMaterialFilledIcon.Edit,
                onclick = (e) => DisplayUtils.ToggleEditListMode(),
                hidden = true
            });


            Tools.Add(new NavTool()
            {
                tool = ToolTypes.AddProductSet,
                tip = "Add Set",
                icon = MdiIcon.FormatListGroupPlus,
                onclick = (e) => {
                    GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.AddProductSet, 0);
                },
                hidden = true
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.EditProductSets,
                tip = "Edit Sets",
                icon = GoogleMaterialFilledIcon.Edit,
                onclick = (e) => DisplayUtils.ToggleEditListMode(),
                hidden = true
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.ViewProductSet,
                tip = "View Set",
                icon = MdiIcon.FormatListGroup,
                onclick = (e) => NavigateTo(VPageTypes.ProductSets),
                hidden = true
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.AddProductSetItem,
                tip = "Add Set Item",
                icon = MdiIcon.FormatListGroupPlus,
                onclick = (e) => {
                    GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.AddListItem, 0);
                },
                hidden = true
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.EditProductSetItems,
                tip = "Edit Sets Items",
                icon = MdiIcon.PlaylistEdit,
                onclick = (e) => DisplayUtils.ToggleEditItemsMode(),
                hidden = true
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.Settings,
                tip = "Settings",
                icon = Ionicon.SettingsSharp,
                onclick = (e) => GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.ShowHideSettings, true),
                right = true,
                hidden = true
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.ShowButtonTitles,
                tip = "Display Button Titles",
                icon = GoogleMaterialFilledIcon.QuestionMark,
                onholdDown = () => DisplayUtils.ShowEditorButtonTitles = true,
                onholdUp = () => DisplayUtils.ShowEditorButtonTitles = false,
                small = true,
                editorButton = true,
                suppressTitle = true
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.AddProduct,
                tip = "New Product",
                icon = MdiIcon.PlusCircleOutline,
                editorButton = true,
                titleArrows = [90,0,10,70]
            });
            
            Tools.Add(new NavTool()
            {
                tool = ToolTypes.ScanBarcode,
                tip = "Scan Barcode",
                icon = MdiIcon.Barcode,
                hasTimer = true,
                timeOut = 3000,
                editorButton = true,
                titleArrows = [45,30,40,30]
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.FinishEditProduct,
                tip = "Save Product",
                icon = MdiIcon.CheckboxMarkedCircleOutline,
                editorButton = true,
                right = true,
                titleArrows = [45,92,120,30]
            });
            
            Tools.Add(new NavTool()
            {
                tool = ToolTypes.EditorPrevButton,
                tip = "Previous Item",
                icon = MdiIcon.ArrowLeftBoldCircleOutline,
                editorButton = true,
                right = true,
                titleArrows = [90,64,79,70]
            });
            
            Tools.Add(new NavTool()
            {
                tool = ToolTypes.EditorNextButton,
                tip = "Next Item",
                icon = MdiIcon.ArrowRightBoldCircleOutline,
                editorButton = true,
                right = true,
                titleArrows = [45,18,46,30]
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.EditorCloseButton,
                tip = "Close Editor",
                icon = MdiIcon.CloseCircleOutline,
                editorButton = true,
                right = true,
                titleArrows = [90,1,5,70]
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.AddLocation,
                tip = "New Location",
                icon = MdiIcon.PlusCircleOutline,
                editorButton = true,
                titleArrows = [45,0,10,30]
            });

            Tools.Add(new NavTool()
            {
                tool = ToolTypes.SaveLocation,
                tip = "Save Location",
                icon = MdiIcon.CheckboxMarkedCircleOutline,
                right = true,
                editorButton = true,
                titleArrows = [45,92,120,30]
            });
        }
    }
}
