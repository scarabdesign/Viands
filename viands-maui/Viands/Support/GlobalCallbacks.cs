namespace Viands.Support
{
    public static class GlobalCallbacks
    {
        public enum CBKeys
        {
            UpdateUser,
            ShowHideSettings,
            RefreshLists,
            WindowResized,
            RefreshState,
            RefreshMainLayout,
            ShowHideLogin,
            EditProductType,
            EditNextProductType,
            EditPrevProductType,
            EditNextLocation,
            EditPrevLocation,
            EditLocation,
            CloseDialog,
            DialogClosed,
            Navigate,
            SetTitle,
            AddListItem,
            CancelEdit,
            SaveEdit,
            SaveAndAdd,
            EditItemsModeToggled,
            EditItemsModeExited,
            EditListModeToggled,
            EditListModeExited,
            ToolsLockedClosed,
            ToolsLockedOpen,
            ToolsUnlocked,
            NewLocationAdded,
            AddTemplate,
            EditTemplate,
            ViewTemplateItems,
            AddProductSet,
            ViewProductSetItems,
            AddProductSetItem,
            SelectProductSets,
            ProductSetsSelected,
            OpenSelectProductSets,
            EditProductTypeByUPC,
            ProductTypesSelected,
            BarcodeResults,
            RequestLineItemForUPC,
            ResponseLineItemForUPC,
            RequestUPCForProductType,
            ResponseUPCForProductType,
            ShowNavButtonTitles,
            HideNavButtonTitles,
            ShowEditorButtonTitles,
            HideEditorButtonTitles
        }

        public delegate void TriggerCallback(CBKeys key, dynamic args);
        public static event TriggerCallback OnTriggerCallback;

        public static void Trigger(CBKeys key, dynamic args)
        {
            OnTriggerCallback?.Invoke(key, args);
        }

        public enum ResponseTypes
        {
            AddItems,
            Navigate,
            EditProductType,
            EditLocation,
            Templates,
            SetSelected
        }

        public class GeneralResponse 
        {
            public ResponseTypes ResponseType;
            public string UrlArg;
            public string StringArg;
            public int? IntArg;
            public int?[] IntArrayArg;
            public bool? BoolArg;
        }

        public class EditProductTypeResponse
        {
            public ResponseTypes ResponseType = ResponseTypes.EditProductType;
            public int ProductTypeId;
            public int ListId;
            public int ListItemId;
            public int Quantity;
            public string UPC;
            public bool IsFirst;
            public bool IsLast;
            public bool FromBarcode;

            public bool IsBlank()
            {
                return ProductTypeId == 0 && ListId == 0 && ListItemId == 0;
            }
        }

        public class EditLocationResponse
        {
            public ResponseTypes ResponseType = ResponseTypes.EditLocation;
            public bool Singleton;
            public int LocationId;
            public bool AddEdit;
            public bool IsFirst;
            public bool IsLast;
        }

        public class BarcodeResponse
        {
            public List<string> Barcodes;
            public int ProductTypeId;
            public int ListId;
            public int ListItemId;
        }

        public class ProductSetResponse
        {
            public int SetId;
            public string Name;
        }

        public class ProductSetsSelectedResponse
        {
            public bool CreateNew;
            public int[] SetIds;
        }

        public class ProductTypessSelectedResponse
        {
            public int[] ProductTypeIds;
        }
    }
}
