using Newtonsoft.Json;

namespace Viands.Support
{
    public static class Constants
    {
        [Flags]
        public enum DemarcationOptions
        {
            Unset = -0x1,
            Unmarked = 0x0,
            Checked = 0x1,
            ForwardSlash = 0x2,
            BackSlash = 0x4,
            X = 0x8,
            Asterisk = 0x10,
            StrikeThrough = 0x20,
            Violet = 0x40,
            BlueViolet = 0x80,
            Blue = 0x100,
            BlueGreen = 0x200,
            Green = 0x400,
            YellowGreen = 0x800,
            Yellow = 0x1000,
            YellowOrange = 0x2000,
            Orange = 0x4000,
            RedOrange = 0x8000,
            Red = 0x10000,
            RedViolet = 0x20000
        }

        public enum SortTypes
        {
            Product = 0,
            Price = 1,
            Location = 2
        }

        public const string VirtualCloudPath = "__viands_cloud_backup";
    }

    public class SetBrief
    {
        public string SetName;
        public int SetId;
        public int Order;
        public List<ProductTypeBrief> ProductList;
    }

    public class ProductTypeBrief
    {
        public string ProductTypeName { get; set; }
        public int ProductTypeId { get; set; }
    }

    public static class ListMeta
    {
        public static string ListLocationOrder = "ListLocationOrder";
    }

    public class CreateProductFromSelection
    {
        public string Name;
        public int SetId;
        public int ProductId;
        public int NewToSet;
    }

    public class JavaScriptMessageRequest
    {
        public string Method;
        public dynamic Message;
    }

    public class ProductAssignmentRequest
    {
        public int ListItemId { get; set; }
        public int ProductTypeId { get; set; }
        public int Quantity { get; set; }
    }

    public class ProductMatchRequest
    {
        public int ListItemId;
        public string CurrentVal;
    }

    public class SelectSetRequest
    {
        public int ListId;
        public bool ShowAddNew;
    }

    public class MenuItemResponse
    {
        public string Name { get; set; }
        public int ProductTypeId { get; set; }
        public string SetName { get; set; }
        public int SetId { get; set; }
        public int SetCount { get; set; }
        public int UnsavedProductCount { get; set; }
        public int HasChanged { get; set; }
        public int SetNameChanged { get; set; }
        public int NewToSet { get; set; }
        public int SetItemsDirty { get; set; }
        public int HasUnimportedItems { get; set; }
        public int NewSetItemsCount { get; set; }
        public int UnimportedItemsCount { get; set; }
        public string CurrentTarget { get; set; }
        public List<MenuItemResponse> Products { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class MenuItemResult
    {
        public string CurrentTarget { get; set; }
        public int SetId { get; set; }
        public string SetName { get; set; }
        public int ProductTypeId { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class ViandsCloudBackupStatus
    {
        public const string CloudSuccess = "cloud_success";
        public const string CloudFailed = "cloud_failed";
        public const string LocalSuccess = "local_success";
        public const string LocalFailed = "local_failed";
        public const string RemoteAvailable = "remote_database";
        public const string RemoteSuccess = "remote_success";
        public const string RemoteFailed = "remote_failed";
    }

    public class BackupStatusFilenames
    {
        public const string CloudSuccess = "c1";
        public const string CloudFailed = "c0";
        public const string LocalSuccess = "l1";
        public const string LocalFailed = "l0";
        public const string RemoteAvailable = "r2";
        public const string RemoteSuccess = "r1";
        public const string RemoteFailed = "r0";
    }

    public enum EnvironmentsType
    {
        Local,
        Public
    }

    public class AppSettings
    {
        public EnvironmentsType env { get; set; }
        public Dictionary<EnvironmentsType, string> APIEndpoint { get; set; }
    }
}