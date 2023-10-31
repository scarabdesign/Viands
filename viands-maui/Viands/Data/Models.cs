using System.Diagnostics;
using Viands.Support;

namespace Viands.Data
{
    public class Lists
    {
        public async static Task<List<v_lists>> GetLists(string owner_id)
        {
            var lists = await Constants.Database?.connection?
                .Table<v_lists>()
                .Where(i => i.owner_id == owner_id && i.is_template == false && i.is_set == false)
                .OrderBy(i => i.order)
                .ThenBy(i => i.date_created)
                .ToListAsync();

            return lists;
        }

        public async static Task<List<v_lists>> GetTemplates(string owner_id)
        {
            var lists = await Constants.Database?.connection?
                .Table<v_lists>()
                .Where(i => i.owner_id == owner_id && i.is_template == true && i.is_set == false)
                .OrderBy(i => i.order)
                .ThenBy(i => i.date_created)
                .ToListAsync();

            return lists;
        }

        public async static Task<List<v_lists>> GetSets(string owner_id)
        {
            var lists = await Constants.Database?.connection?
                .Table<v_lists>()
                .Where(i => i.owner_id == owner_id && i.is_template == false && i.is_set == true)
                .OrderBy(i => i.name)
                .ThenBy(i => i.date_created)
                .ToListAsync();

            return lists;
        }

        public async static Task<List<v_lists>> GetSets(string owner_id, int[] ids)
        {
            var lists = await Constants.Database?.connection?
                .Table<v_lists>()
                .Where(i => i.owner_id == owner_id && i.is_template == false && i.is_set == true && ids.Contains(i.id))
                .OrderBy(i => i.name)
                .ThenBy(i => i.date_created)
                .ToListAsync();

            return lists;
        }

        public async static Task<v_lists> GetSet(string owner_id, int setId)
        {
            return await Constants.Database?.connection?
                .Table<v_lists>()
                .FirstOrDefaultAsync(i => i.owner_id == owner_id && i.is_template == false && i.is_set == true && i.id == setId);
        }

        public async static Task<v_lists> GetList(int id, string owner_id)
        {
            return await Constants.Database?.connection?.Table<v_lists>().Where(i => i.id == id && i.owner_id == owner_id).FirstOrDefaultAsync();
        }

        public async static Task<int[]> ClearItemsForList(int listid)
        {
            return await ListItems.DeleteListItems(await ListItems.GetListItems(listid));
        }

        public async static Task<bool> ListNameExists(string listname, string owner_id, bool template, bool productset)
        {
            return await Constants.Database?.connection?
                .ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM v_lists " +
                    "WHERE is_template = " + (template ? "1" : "0") + " " +
                    "AND is_set = " + (productset ? "1" : "0") + " " +
                    "AND owner_id = \"" + owner_id + "\" " +
                    "AND name = \"" + listname + "\""
                ) > 0;
        }

        public async static Task<int> GetNextListOrderValue(bool template, bool productset, string owner_id)
        {
            return await GetMaxListOrderValue(template, productset, owner_id) + 1;
        }

        public async static Task<int> GetMaxListOrderValue(bool template, bool productset, string owner_id)
        {
            return await Constants.Database?.connection?
                .ExecuteScalarAsync<int>(
                    "SELECT MAX(\"order\") FROM v_lists " +
                    "WHERE \"order\" IS NOT NULL " +
                    "AND is_template = " + (template ? "1" : "0") + " " +
                    "AND is_set = " + (productset ? "1" : "0") + " " +
                    "AND owner_id = \"" + owner_id + "\""
                );
        }

        public async static Task<int> GetMinListOrderValue(bool template, bool productset, string owner_id)
        {
            return await Constants.Database?.connection?
                .ExecuteScalarAsync<int>(
                    "SELECT MIN(\"order\") FROM v_lists " +
                    "WHERE \"order\" IS NOT NULL " +
                    "AND is_template = " + (template ? "1" : "0") + " " +
                    "AND is_set = " + (productset ? "1" : "0") + " " +
                    "AND owner_id = \"" + owner_id + "\""
                );
        }

        public async static Task<List<int>> SaveLists(List<v_lists> lists)
        {
            var tasks = lists.Select(UpsertList);
            var result = await Task.WhenAll(tasks);
            return result.ToList();
        }

        public async static Task<int> UpsertList(v_lists list)
        {
            if (list == null) return 0;
            list.name = System.Web.HttpUtility.HtmlEncode(System.Web.HttpUtility.HtmlDecode(list.name));
            list.date_updated = DateTime.UtcNow;
            if (list.id != 0)
            {
                return await ViandsDatabase.Update(list);
            }

            list.date_created = DateTime.UtcNow;
            list.owner_id = await LoginUtils.GetCurrentUserApiKey();
            list.order = list.order == 0 ? (await ViandsDatabase.GetMaxOrder("v_lists")) + 1 : list.order;
            return await ViandsDatabase.Insert(list);
        }

        public async static Task<List<v_listitems>> GetListItemsInListForLocation(int listid, int locationid)
        {
            var list = await ListItems.GetListItems(listid);
            if (list == null) return null;
            var prodTypes = await ProductTypes.GetProductTypesForLocation(locationid);
            if (prodTypes == null) return null;
            var typeIds = prodTypes.Select(p => p.id);
            return list?.Where(li => typeIds.Contains(li.producttype_id)).ToList();
        }

        public async static Task<int> DeleteList(v_lists list)
        {
            return await ViandsDatabase.Delete(list);
        }
    }

    public class Users
    {
        public async static Task<List<v_users>> GetUsers()
        {
            return await Constants.Database?.connection?.Table<v_users>().ToListAsync();
        }

        public async static Task<v_users> GetCurrentUser()
        {
            try
            {
                return await Constants.Database?.connection?.Table<v_users>().Where(i => i.current == true).FirstOrDefaultAsync();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return null;
        }

        public async static Task UnsetCurrentUser()
        {
            await Constants.Database?.connection?.ExecuteAsync("update v_users set current = false");
        }

        public async static Task<v_users> GetUserByEmail(string email)
        {
            return await Constants.Database?.connection?.Table<v_users>().Where(i => i.email == email).FirstOrDefaultAsync();
        }

        public async static Task<v_users> GetUser(int id)
        {
            return await Constants.Database?.connection?.Table<v_users>().Where(i => i.id == id).FirstOrDefaultAsync();
        }

        public async static Task<int> UpsertUser(v_users user)
        {
            user.name = System.Web.HttpUtility.HtmlEncode(System.Web.HttpUtility.HtmlDecode(user.name));
            user.date_updated = DateTime.UtcNow;
            if (user.id != 0)
            {
                return await ViandsDatabase.Update(user);
            }

            user.date_created = DateTime.UtcNow;
            return await ViandsDatabase.Insert(user);
        }

        public async static Task<int> DeleteUser(v_users user)
        {
            return await ViandsDatabase.Delete(user);
        }

    }

    public class ListItems
    {
        public async static Task<List<v_listitems>> GetListItems(int listid)
        {
            return await Constants.Database?.connection?
                .Table<v_listitems>()
                .Where(i => i.list_id == listid)
                .OrderBy(i => i.order)
                .ToListAsync();
        }

        public async static Task<List<v_listitems>> GetListItems(int[] listitemids)
        {
            return await Constants.Database?.connection?
                .Table<v_listitems>()
                .Where(i => listitemids.Contains(i.list_id))
                .OrderBy(i => i.order)
                .ToListAsync();
        }

        public async static Task SetListItemQuantity(int listitemid, int quantity)
        {
            var listitem = await GetListItem(listitemid);
            if (listitem == null) return;
            listitem.quantity = quantity;
            await UpsertListItem(listitem);
        }

        public async static Task<List<v_listitems>> GetListItemsForProductType(int producttypeid)
        {
            return await Constants.Database?.connection?
                .Table<v_listitems>()
                .Where(i => i.producttype_id == producttypeid)
                .OrderBy(i => i.order)
                .ToListAsync();
        }

        public async static Task<v_listitems> GetItemInListForProductType(int listid, int producttypeid)
        {
            return await Constants.Database?.connection?
                .Table<v_listitems>()
                .Where(i => i.producttype_id == producttypeid)
                .Where(i => i.list_id == listid)
                .OrderBy(i => i.order)
                .FirstOrDefaultAsync();
        }

        public async static Task<v_listitems> GetListItemByUPC(int listid, string upc)
        {
            var prodTypes = await ProductTypes.GetProducttypesForUPC(upc);
            if(prodTypes != null && prodTypes.Count > 0)
            {
                return await GetItemInListForProductType(listid, prodTypes[0].id);
            }
            return null;
        }


        public async static Task<List<int>> ImportListItems(List<v_listitems> lists)
        {
            var tasks = lists.Select(ViandsDatabase.Insert);
            var results = await Task.WhenAll(tasks);
            return results.ToList();
        }

        public async static Task<List<int>> SaveListItems(List<v_listitems> lists)
        {
            var tasks = lists.Select(UpsertListItem);
            var results = await Task.WhenAll(tasks);
            return results.ToList();
        }

        public async static Task<v_listitems> GetListItem(int id)
        {
            return await Constants.Database?.connection?.Table<v_listitems>().Where(i => i.id == id).FirstOrDefaultAsync();
        }

        public async static Task<int> UpsertListItem(v_listitems listitem)
        {
            listitem.date_updated = DateTime.UtcNow;
            if (listitem.id != 0)
            {
                return await ViandsDatabase.Update(listitem);
            }
            listitem.order = listitem.order == 0 ? (await ViandsDatabase.GetMaxOrder("v_listitems") + 1): listitem.order;
            if (listitem.quantity == 0)
            {
                listitem.quantity = 1;
            }
            listitem.date_created = DateTime.UtcNow;
            return await ViandsDatabase.Insert(listitem);
        }

        public async static Task<int[]> DeleteListItems(List<v_listitems> listitems)
        {
            var tasks = listitems.Select(DeleteListItem);
            var results = await Task.WhenAll(tasks);
            return results;
        }

        public async static Task<int> DeleteListItem(v_listitems listitem)
        {
            return await ViandsDatabase.Delete(listitem);
        }
    }

    public class ProductTypes
    {
        public async static Task<List<v_producttypes>> GetProductTypes(int limit = -1)
        {
            return await Constants.Database?.connection?.Table<v_producttypes>()
                .OrderBy(i => i.typename)
                .Take(limit == -1 ? 999999: limit)
                .ToListAsync();
        }

        public async static Task<List<v_producttypes>> GetProductTypes(int[] ids)
        {
            return await Constants.Database?.connection?.Table<v_producttypes>()
                .Where(i => ids.Contains(i.id))
                .OrderBy(i => i.typename).ToListAsync();
        }

        //return all product_types that don't exist in the given list
        public async static Task<List<v_producttypes>> GetProductTypesAvailableForList(int listid)
        {
            var listitems = await ListItems.GetListItems(listid);
            var prodTypes = listitems.Select(li => li.producttype_id).ToList();
            return await Constants.Database?.connection?.Table<v_producttypes>()
                .Where(pt => !prodTypes.Contains(pt.id))
                .OrderBy(pt => pt.typename).ToListAsync();
        }

        public async static Task<List<v_producttypes>> GetProductTypesInList(int listid)
        {
            var listitems = await ListItems.GetListItems(listid);
            var prodTypes = listitems.Select(li => li.producttype_id).ToList();
            return await Constants.Database?.connection?.Table<v_producttypes>()
                .Where(pt => prodTypes.Contains(pt.id))
                .OrderBy(pt => pt.typename).ToListAsync();
        }

        public async static Task<List<v_producttypes>> GetProducttypesForUPC(string upc)
        {
            var prs = await Constants.Database?.connection?.Table<v_products>()
                .Where(pr => pr.upc == upc)
                .ToListAsync();

            var ptids = prs.Select(pr => pr.id).ToList();
            return await Constants.Database?.connection?.Table<v_producttypes>()
                .Where(pt => ptids.Contains(pt.product_id))
                .OrderBy(pt => pt.typename)
                .ToListAsync();
        }

        public async static Task<List<v_producttypes>> FilterProductTypes(string searchTerm)
        {
            return await Constants.Database?.connection?.Table<v_producttypes>()
                .Where(i => i.typename.ToLower().Contains(searchTerm.ToLower()))
                .OrderBy(i => i.typename).ToListAsync();
        }

        public async static Task<List<v_producttypes>> GetProductTypesForLocation(int locationid)
        {
            return await Constants.Database?.connection?.Table<v_producttypes>()
                .Where(i => i.location_id == locationid)
                .OrderBy(i => i.typename).ToListAsync();
        }

        public async static Task<v_producttypes> GetProductType(int id)
        {
            return await Constants.Database?.connection?.Table<v_producttypes>()
                .Where(i => i.id == id).FirstOrDefaultAsync();
        }

        public async static Task<int[]> UpsertProductTypes(List<v_producttypes> producttypes)
        {
            var tasks = producttypes.Select(UpsertProductType);
            var results = await Task.WhenAll(tasks);
            return results;
        }

        public async static Task<int> UpsertProductType(v_producttypes producttype)
        {
            producttype.typename = System.Web.HttpUtility.HtmlEncode(System.Web.HttpUtility.HtmlDecode(producttype.typename));
            producttype.notes = System.Web.HttpUtility.HtmlEncode(System.Web.HttpUtility.HtmlDecode(producttype.notes));
            producttype.date_updated = DateTime.UtcNow;
            if (producttype.id != 0)
            {
                return await ViandsDatabase.Update(producttype);
            }

            producttype.date_created = DateTime.UtcNow;
            return await ViandsDatabase.Insert(producttype);
        }

        public async static Task<int> DeleteProductType(v_producttypes producttype)
        {
            await ClearProductTypeDetails(producttype.price_id, producttype.product_id);
            return await ViandsDatabase.Delete(producttype);
        }

        public async static Task ClearProductTypeDetails(int priceid, int producttypeid)
        {
            if (priceid != 0)
            {
                await Prices.DeletePrice(
                    await Prices.GetPrice(priceid)
                );
            }
            if (producttypeid != 0)
            {
                await Products.DeleteProduct(
                    await Products.GetProduct(producttypeid)
                );
            }
        }
    }

    public class Locations
    {
        public async static Task<List<v_locations>> GetLocations()
        {
            return await Constants.Database?.connection?.Table<v_locations>().ToListAsync();
        }

        public async static Task<List<v_locations>> FilterLocations(string searchTerm)
        {
            var _searchTerm = searchTerm.ToLower();
            return await Constants.Database?.connection?.Table<v_locations>()
                .Where(i => i.name.ToLower().Contains(_searchTerm))
                .ToListAsync();
        }

        public async static Task<v_locations> GetLocation(int id)
        {
            return await Constants.Database?.connection?.Table<v_locations>().Where(i => i.id == id).FirstOrDefaultAsync();
        }

        public async static Task<List<int>> SaveLocations(List<v_locations> locations)
        {
            var tasks = locations.Select(UpsertLocation);
            var results = await Task.WhenAll(tasks);
            return results.ToList();
        }

        public async static Task<int> UpsertLocation(v_locations location)
        {
            location.name = System.Web.HttpUtility.HtmlEncode(System.Web.HttpUtility.HtmlDecode(location.name));
            location.address = System.Web.HttpUtility.HtmlEncode(System.Web.HttpUtility.HtmlDecode(location.address));
            location.city = System.Web.HttpUtility.HtmlEncode(System.Web.HttpUtility.HtmlDecode(location.city));
            location.zip = System.Web.HttpUtility.HtmlEncode(System.Web.HttpUtility.HtmlDecode(location.zip));
            location.description = System.Web.HttpUtility.HtmlEncode(System.Web.HttpUtility.HtmlDecode(location.description));
            location.date_updated = DateTime.UtcNow;
            location.order = location.order == 0 ? (await ViandsDatabase.GetMaxOrder("v_locations") + 1) : location.order;
            if (location.id != 0)
            {
                return await ViandsDatabase.Update(location);
            }

            location.date_created = DateTime.UtcNow;
            return await ViandsDatabase.Insert(location);
        }

        public async static Task<int> DeleteLocation(v_locations location)
        {
            return await ViandsDatabase.Delete(location);
        }
    }

    public class Prices
    {
        public async static Task<List<v_prices>> GetPrices()
        {
            return await Constants.Database?.connection?.Table<v_prices>().ToListAsync();
        }

        public async static Task<v_prices> GetPrice(int id)
        {
            return await Constants.Database?.connection?.Table<v_prices>().Where(i => i.id == id).FirstOrDefaultAsync();
        }

        public async static Task<int> UpsertPrice(v_prices price)
        {
            price.product_size = System.Web.HttpUtility.HtmlEncode(System.Web.HttpUtility.HtmlDecode(price.product_size));
            price.per_unit_type = System.Web.HttpUtility.HtmlEncode(System.Web.HttpUtility.HtmlDecode(price.per_unit_type));
            price.date_updated = DateTime.UtcNow;
            if (price.id != 0)
            {
                return await ViandsDatabase.Update(price);
            }

            price.date_created = DateTime.UtcNow;
            return await ViandsDatabase.Insert(price);
        }

        public async static Task<int> DeletePrice(v_prices price)
        {
            return await ViandsDatabase.Delete(price);
        }
    }

    public class Products
    {
        public async static Task<List<v_products>> GetProducts()
        {
            return await Constants.Database?.connection?.Table<v_products>().ToListAsync();
        }

        public async static Task<v_products> GetProduct(int id)
        {
            return await Constants.Database?.connection?.Table<v_products>().Where(i => i.id == id).FirstOrDefaultAsync();
        }

        public async static Task<v_products> GetProductByUPC(string upc)
        {
            return await Constants.Database?.connection?.Table<v_products>().Where(i => i.upc == upc).FirstOrDefaultAsync();
        }

        public async static Task<int> UpsertProduct(v_products product)
        {
            product.brandname = System.Web.HttpUtility.HtmlEncode(System.Web.HttpUtility.HtmlDecode(product.brandname));
            product.description = System.Web.HttpUtility.HtmlEncode(System.Web.HttpUtility.HtmlDecode(product.description));
            product.date_updated = DateTime.UtcNow;
            if (product.id != 0)
            {
                return await ViandsDatabase.Update(product);
            }

            product.date_created = DateTime.UtcNow;
            return await ViandsDatabase.Insert(product);
        }

        public async static Task<int> DeleteProduct(v_products product)
        {
            return await ViandsDatabase.Delete(product);
        }
    }

    public class Settings
    {
        public async static Task<List<v_settings>> GetSettings(string owner_id)
        {
            return await Constants.Database?.connection?.Table<v_settings>().Where(i => i.owner_id == owner_id).ToListAsync();
        }

        public async static Task<v_settings> GetSetting(int id)
        {
            return await Constants.Database?.connection?.Table<v_settings>().Where(i => i.id == id).FirstOrDefaultAsync();
        }

        public async static Task<v_settings> GetSettingByKey(string owner_id, string key)
        {
            return await Constants.Database?.connection?.Table<v_settings>().Where(i => i.key == key && i.owner_id == owner_id).FirstOrDefaultAsync();
        }

        public async static Task<List<int>> UpsertSettings(List<v_settings> settings)
        {
            var tasks = settings.Select(UpsertSetting);
            var results = await Task.WhenAll(tasks);
            return results.ToList();
        }

        public async static Task<int> UpsertSetting(v_settings setting)
        {
            setting.date_updated = DateTime.UtcNow;
            if (setting.id != 0)
            {
                return await ViandsDatabase.Update(setting);
            }

            setting.date_created = DateTime.UtcNow;
            return await ViandsDatabase.Insert(setting);
        }

        public async static Task<int> DeleteSetting(v_settings setting)
        {
            return await ViandsDatabase.Delete(setting);
        }
    }
}
