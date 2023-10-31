using SQLite;
using SQLiteNetExtensions.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Viands.Data
{
    public static class Constants
    {
        public const string DatabaseFilename = "Viands.db3";
        public static ViandsDatabase Database { get; set; }

        public const SQLiteOpenFlags Flags =
            // open the database in read/write mode
            SQLiteOpenFlags.ReadWrite |
            // create the database if it doesn't exist
            SQLiteOpenFlags.Create |
            // enable multi-threaded database access
            SQLiteOpenFlags.SharedCache;

        public static string APIEndpoint;

        public static string DatabasePath
        {
            get
            {
                try
                {
                    return Path.Combine(DatabaseFolder, DatabaseFilename);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                return null;
            }
        }

        public static string DatabaseFolder
        {
            get
            {
                try
                {
                    return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                return null;
            }
        }

        public async static void InitDatabase(Action callback)
        {
            Database = new ViandsDatabase();
            await Database.InitComponents();
            callback?.Invoke();
        }

        public static string DatabaseBackupFolder
        {
            get
            {
                try
                {
                    return Path.Combine(DatabaseFolder, "backup");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                return null;
            }
        }
        public static string DatabaseBackupPath(string backupname)
        {
            try
            {
                return Path.Combine(DatabaseBackupFolder, backupname + ".db3");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }
    }

    [Table("v_users")]
    public class v_users
    {
        [Column("id"), PrimaryKey, AutoIncrement]
        public int id { get; set; }

        [Column("apikey"), Unique]
        public string apikey { get; set; }

        [Column("name"), Required, SQLite.MaxLength(250), Unique]
        public string name { get; set; }

        [Column("password"), Required]
        public string password { get; set; }

        [Column("email"), Required, EmailAddress, Unique]
        public string email { get; set; }

        [Column("current")]
        public bool current { get; set; }

        [Column("date_created")]
        public DateTime date_created { get; set; }

        [Column("date_updated")]
        public DateTime date_updated { get; set; }
        //public U Create<U, V>(V constructorArgs)
        //{
        //    var instance = (U)Activator.CreateInstance(typeof(U), constructorArgs);
        //    return instance;
        //}
    }

    [Table("v_lists")]
    public class v_lists
    {
        [Column("id"), PrimaryKey, AutoIncrement]
        public int id { get; set; }

        [Column("name"), Unique]
        public string name { get; set; }

        [Column("order")]
        public int order { get; set; }

        [Column("owner_id"), Required]
        public string owner_id { get; set; }

        [Column("description")]
        public string description { get; set; }

        [Column("is_template")]
        public bool is_template { get; set; }

        [Column("is_set")]
        public bool is_set { get; set; }

        [Column("list_meta")]
        public string list_meta { get; set; }

        [Column("date_created")]
        public DateTime date_created { get; set; }

        [Column("date_updated")]
        public DateTime date_updated { get; set; }
    }

    [Table("v_listitems")]
    public class v_listitems
    {
        [Column("id"), PrimaryKey, AutoIncrement]
        public int id { get; set; }

        [Column("list_id"), ForeignKey(typeof(v_lists))]
        public int list_id { get; set; }

        [Column("producttype_id"), ForeignKey(typeof(v_producttypes))]
        public int producttype_id { get; set; }

        [Column("order")]
        public int order { get; set; }

        [Column("quantity")]
        public int quantity { get; set; }

        [Column("demarcated")]
        public int demarcation { get; set; }

        [Column("date_created")]
        public DateTime date_created { get; set; }

        [Column("date_updated")]
        public DateTime date_updated { get; set; }
    }

    [Table("v_producttypes")]
    public class v_producttypes
    {
        [Column("id"), PrimaryKey, AutoIncrement]
        public int id { get; set; }

        [Column("typename")]
        public string typename { get; set; }

        [Column("product_id"), ForeignKey(typeof(v_products))]
        public int product_id { get; set; }

        [Column("location_id"), ForeignKey(typeof(v_locations))]
        public int location_id { get; set; }
        
        [Column("price_id"), ForeignKey(typeof(v_prices))]
        public int price_id { get; set; }

        [Column("notes")]
        public string notes { get; set; }

        [Column("date_created")]
        public DateTime date_created { get; set; }
        
        [Column("date_updated")]
        public DateTime date_updated { get; set; }

    }

    [Table("v_locations")]
    public class v_locations 
    {
        [Column("id"), PrimaryKey, AutoIncrement]
        public int id { get; set; }

        [Column("name")]
        public string name { get; set; }

        [Column("logo")]
        public string logo { get; set; }

        [Column("address")]
        public string address { get; set; }

        [Column("city")]
        public string city { get; set; }

        [Column("zip")]
        public string zip { get; set; }

        [Column("coordinates")]
        public string coordinates { get; set; }

        [Column("description")]
        public string description { get; set; }

        [Column("order")]
        public int order { get; set; }

        [Column("date_created")]
        public DateTime date_created { get; set; }

        [Column("date_updated")]
        public DateTime date_updated { get; set; }
    }

    [Table("v_prices")]
    public class v_prices
    {
        [Column("id"), PrimaryKey, AutoIncrement]
        public int id { get; set; }

        [Column("product_size")]
        public string product_size { get; set; }

        [Column("price")]
        public int price { get; set; } //in cents

        [Column("per_unit_type")]
        public string per_unit_type { get; set; }

        [Column("on_sale")]
        public bool on_sale { get; set; }

        [Column("date_created")]
        public DateTime date_created { get; set; }

        [Column("date_updated")]
        public DateTime date_updated { get; set; }
    }

    [Table("v_products")]
    public class v_products
    {
        [Column("id"), PrimaryKey, AutoIncrement]
        public int id { get; set; }

        [Column("brandname")]
        public string brandname { get; set; }

        [Column("upc")]
        public string upc { get; set; }

        [Column("description")]
        public string description { get; set; }

        [Column("date_created")]
        public DateTime date_created { get; set; }

        [Column("date_updated")]
        public DateTime date_updated { get; set; }
    }

    [Table("v_settings")]
    public class v_settings
    {
        [Column("id"), PrimaryKey, AutoIncrement]
        public int id { get; set; }

        [Column("owner_id"), Required]
        public string owner_id { get; set; }

        [Column("key")]
        public string key { get; set; }

        [Column("value")]
        public string value { get; set; }

        [Column("date_created")]
        public DateTime date_created { get; set; }

        [Column("date_updated")]
        public DateTime date_updated { get; set; }
    }
}
