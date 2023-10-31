using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Viands.Support;

namespace Viands.Data.ViewModels
{

    public class VLists
    {
        public static async Task<List<VList>> GetListsForUser(string owner_id)
        {
            return await InitList(await Lists.GetLists(owner_id));
        }

        public static async Task<List<VList>> GetTemplatesForUser(string owner_id)
        {
            return await InitList(await Lists.GetTemplates(owner_id));
        }

        public static async Task<List<VList>> GetSetsForUser(string owner_id)
        {
            return await InitList(await Lists.GetSets(owner_id));
        }

        public static async Task<List<VList>> InitList(List<v_lists> lists)
        {
            var listentries = lists.Select(VList.Init);
            var tasks = await Task.WhenAll(listentries);
            var retList = tasks.ToList();
            return retList;
        }

        public static async Task<List<KeyValuePair<int, string>>> GetTemplateSelection(string owner_id)
        {
            var list = await Lists.GetTemplates(owner_id);
            var retList = list.OrderBy(l => l.order).Select(l => new KeyValuePair<int, string>(l.id, l.name)).ToList();
            retList.Insert(0, new KeyValuePair<int, string>(0, string.Empty));
            return retList;
        }

        public static async Task<List<SetBrief>> GetSetsBrief(string owner_id, int[] setIds)
        {
            var sets = await Lists.GetSets(owner_id, setIds);
            var tasks = sets.Select(s => GetSetBrief(owner_id, s.id));
            var setArray = await Task.WhenAll(tasks);
            return [.. setArray];
        }

        public static async Task<SetBrief> GetSetBrief(string owner_id, int setId)
        {
            var set = await Lists.GetSet(owner_id, setId);
            if (set == null) return null;
            var items = await ProductTypes.GetProductTypesInList(set.id);
            return new SetBrief
            {
                SetName = set.name,
                SetId = set.id,
                Order = set.order,
                ProductList = items.Select(i => new ProductTypeBrief
                {
                    ProductTypeName = i.typename,
                    ProductTypeId = i.id
                }).ToList()
            };
        }

        public async static Task SaveLists(List<VList> list)
        {
            await Lists.SaveLists(list.Select(i => new v_lists
            {
                id = i.Id,
                name = i.Name,
                description = i.Description,
                owner_id = i.OwnerId,
                is_template = i.IsTemplate,
                is_set = i.IsSet,
                order = i.Order,
                list_meta = i.ListMeta?.ToString(Formatting.None, null) ?? null,
                date_created = i.CreatedDate,
                date_updated = i.UpdatedDate
            }).ToList());
        }
    }

    public class VList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FilteredDescription => GetMenuBrief();
        public int Order { get; set; }
        public string OwnerId { get; set; }
        public bool IsTemplate { get; set; }
        public bool IsSet { get; set; }
        public JObject ListMeta { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public List<VListItem> VListItems { get; set; }

        public List<VListItem> GetSortedList(bool inEditMode, bool inLocationEditMode)
        {
            var allCheckedForLoc = VListItems.GroupBy(i => i.ProductType?.LocationId ?? 0).ToDictionary(i => i.Key, i => i.ToList().All(i => i.Checked ?? false));
            var listLocationOrder = GetListLocationOrder();
            var returnList = new List<VListItem>();
            if (inEditMode)
            {
                returnList = [..
                    VListItems.OrderBy(i => {
                        var locid = i.ProductType?.LocationId ?? 0;
                        if (listLocationOrder != null && listLocationOrder.Contains(locid))
                        {
                            return listLocationOrder.IndexOf(locid) + 1;
                        }
                        return i.ProductType?.Location?.Order ?? 0;
                    }).ThenBy(i => i.Order)
                ];
            }
            else
            {
                returnList = [..
                    VListItems.OrderBy(i => {
                        var locid = i.ProductType?.LocationId ?? 0;
                        if (!inLocationEditMode && allCheckedForLoc.ContainsKey(locid) && allCheckedForLoc[locid])
                        {
                            return 9999 + locid;
                        }
                        if (listLocationOrder != null && listLocationOrder.Contains(locid))
                        {
                            return listLocationOrder.IndexOf(locid) + 1;
                        }
                        return i.ProductType?.Location?.Order ?? 0;
                    }).ThenBy(i => i.Order + ((i.Checked ?? false) ? VListItems.Count : 0))
                ];
            }

            return returnList;
        }

        public async Task SaveListLocationOrder()
        {
            var first = 0;
            var sortedList = GetSortedList(true, false);
            var locationOrder = sortedList.GroupBy(i => i.ProductType?.LocationId ?? 0).ToDictionary(i => i.Key, i => i?.FirstOrDefault()?.ProductType?.Location?.Order ?? first++);
            var orderedLocations = locationOrder.OrderBy(i => i.Value).Select(i => i.Key).ToList();
            var orderMetaString = JsonConvert.SerializeObject(orderedLocations, Formatting.None);
            await SaveListMetaForKey(Support.ListMeta.ListLocationOrder, orderMetaString);
        }

        public string GetMenuBrief()
        {
            if (!string.IsNullOrEmpty(Description) && !IsSet && !IsTemplate)
            {
                var wholeString = new HtmlDocument();
                wholeString.LoadHtml(Description);
                var desc = new List<string>();
                wholeString.DocumentNode?.SelectNodes("//setelementtitle")?.ToList().ForEach(e =>
                {
                    desc.Add(e.InnerText.Replace("&nbsp;", " ").Trim());
                });
                return string.Join(", ", desc);
            }

            return Description;
        }

        public bool AllCheckedForLocation(int locationid)
        {
            var items = VListItems.GroupBy(i => i.ProductType?.LocationId ?? 0).ToDictionary(i => i.Key, i => i.ToList().All(i => i.Checked ?? false));
            return items.ContainsKey(locationid) ? items[locationid] : false;
        }

        public static async Task<bool> AllMarkedForLocation(int listid, int locationid)
        {
            var listItems = await Lists.GetListItemsInListForLocation(listid, locationid);
            return listItems.Any() && listItems.All(i => i.demarcation != 0);
        }

        public void FilterEmpty()
        {
            VListItems = VListItems.Where(i => !string.IsNullOrEmpty(i.ProductType?.TypeName)).ToList();
        }

        public List<int> GetListLocationOrder()
        {
            var listOrderString = GetListMetaWithKey(Support.ListMeta.ListLocationOrder)?.ToObject<string>();
            if(listOrderString != null)
            {
                return JsonConvert.DeserializeObject<List<int>>(listOrderString);
            }
            return null;
        }

        public async Task SaveListMetaForKey(string key, string value)
        {
            if(ListMeta == null)
            {
                ListMeta = [];
            }

            ListMeta[key] = value;
            await VLists.SaveLists(new List<VList>()
            {
                this
            });
        }

        public JToken GetListMetaWithKey(string key)
        {
            return ListMeta != null && ListMeta.HasValues ? ListMeta.GetValue(key) : null;
        }

        public VListItem AddBlankListItem(int listid)
        {
            var newListItem = new VListItem
            {
                Id = 0,
                Demarcation = 0,
                ListId = listid,
                Order = VListItems.Count,
                SelectedForEdit = true,
                ProductType = new VProductType()
            };
            VListItems.Add(newListItem);
            return newListItem;
        }

        public async static Task<VList> Init(v_lists list)
        {
            var listitems = await VListItem.Init(list.id);
            return new VList
            {
                Id = list.id,
                Name = System.Web.HttpUtility.HtmlDecode(list.name),
                Description = System.Web.HttpUtility.HtmlDecode(list.description),
                Order = list.order,
                OwnerId = list.owner_id,
                IsTemplate = list.is_template,
                ListMeta = list.list_meta != null ? JObject.Parse(list.list_meta) : null,
                CreatedDate = list.date_created,
                UpdatedDate = list.date_updated,
                VListItems = listitems
            };
        }

        public async static Task<VList> Init(int listid, string owner_id)
        {
            var list = await Lists.GetList(listid, owner_id);
            if(list == null) return null;
            return await Init(list);
        }

        public async static Task ImportListItems(List<VListItem> list)
        {
            var result = await ListItems.ImportListItems(list.Select(i => new v_listitems
            {
                id = i.Id,
                list_id = i.ListId,
                demarcation = i.Demarcation,
                quantity = i.Quantity,
                order = i.Order,
                producttype_id = i.ProductTypeId,
                date_created = i.CreatedDate,
                date_updated = i.UpdatedDate
            }).ToList());
        }

        public async static Task<List<int>> SaveListItems(List<VListItem> list)
        {
            return await ListItems.SaveListItems(list.Select(i => new v_listitems
            {
                id = i.Id,
                list_id = i.ListId,
                demarcation = i.Demarcation,
                quantity = i.Quantity,
                order = i.Order,
                producttype_id = i.ProductTypeId,
                date_created = i.CreatedDate,
                date_updated = i.UpdatedDate
            }).ToList());
        }

        public async static Task<int> SaveListAsTemplate(int listid, string name, string description, string owner_id)
        {
            var list = await Lists.GetList(listid, owner_id);
            if (list == null) return 0;
            list.id = 0;
            list.name = name;
            list.description = description;
            list.is_template = true;
            list.is_set = true;
            list.order = await Lists.GetNextListOrderValue(true, false, owner_id);
            list.id = await Lists.UpsertList(list);
            await VListItem.CopyAllListItems(listid, list.id);
            return list.id;
        }

        public async static Task<int> SaveTemplateToList(int listid, string name, string description, string owner_id)
        {
            var list = await Lists.GetList(listid, owner_id);
            if (list == null) return 0;
            if (!list.is_template) return 0;
            list.id = 0;
            list.name = name;
            list.description = description;
            list.is_template = false;
            list.is_set = false;
            list.order = await Lists.GetNextListOrderValue(false, false, owner_id);
            list.id = await Lists.UpsertList(list);
            await VListItem.CopyAllListItems(listid, list.id);
            return list.id;
        }

        public async static Task<int> AddSetItemsToList(int listid, int setid, string owner_id)
        {
            var list = await Lists.GetList(listid, owner_id);
            if (list == null) return 0;
            var set = await Lists.GetList(setid, owner_id);
            if (set == null) return 0;
            if (!set.is_set) return 0;
            await VListItem.CopyListItemsOrIncrement(setid, listid);
            return set.id;
        }

        public async static Task<Tuple<int, List<ProductTypeBrief>>> SaveSetAndAddItemToSet(int setid, string name, string owner_id, List<MenuItemResponse> producttypes)
        {
            return await SaveSetAndItems(setid, name, owner_id, producttypes, true);
        }

        public async static Task<Tuple<int, List<ProductTypeBrief>>> SaveSetAndItems(int setid, string name, string owner_id, List<MenuItemResponse> producttypes, bool addtoset = false)
        {
            var newsetid = await SaveList(setid, name, null, false, true, owner_id);
            var producttypeids = producttypes.Select(async producttype =>
            {
                if (producttype.ProductTypeId == 0)
                {
                    producttype.ProductTypeId = await VProductType.SaveProductType(producttype.Name);
                }
                else if (producttype.HasChanged == 1)
                {
                    await VProductType.SaveProductType(producttype.Name, producttype.ProductTypeId);
                }

                if (addtoset && producttype.NewToSet == 1)
                {
                    await AddProductTypeToSet(newsetid, producttype.ProductTypeId);
                }

                return new ProductTypeBrief
                {
                    ProductTypeId = producttype.ProductTypeId,
                    ProductTypeName = producttype.Name,
                };
            });

            var prodids = (await Task.WhenAll(producttypeids)).ToList();
            return new Tuple<int, List<ProductTypeBrief>>(newsetid, prodids);
        }

        public async static Task<int> UpdateSetName (int setid, string name, string owner_id)
        {
            var setlist = new v_lists();
            if (setid > 0)
            {
                setlist = await Lists.GetList(setid, owner_id);
            }

            setlist.name = name;
            return await Lists.UpsertList(setlist);
        }

        public async static Task<int> SaveList(int listid, string name, string description, bool template, bool set, string owner_id)
        {
            var list = new v_lists();
            if (listid > 0)
            {
                list = await Lists.GetList(listid, owner_id);
            }
            list.name = name;
            list.description = description;
            list.is_template = template;
            list.is_set = set;
            list.order = await Lists.GetNextListOrderValue(template, set, owner_id);
            return await Lists.UpsertList(list);
        }

        public async static Task<int[]> ClearItemsForList(int listid)
        {
            return await Lists.ClearItemsForList(listid);
        }

        public async static Task<Dictionary<int, int>> AddProductTypesToList(int listid, Dictionary<int, int> productsAndQuantities)
        {
            var pordidToListid = await Task.WhenAll(
                productsAndQuantities.Select(
                    async paq =>
                    {
                        var listitemid = 0;
                        var listitem = await ListItems.GetItemInListForProductType(listid, paq.Key);
                        if (listitem != null)
                        {
                            listitemid = listitem.id;
                        }
                        return new KeyValuePair<int, int>(
                            paq.Key, await AddUpdateListItem(listid, listitemid, paq.Key, paq.Value)
                        );
                    }
                )
            );

            return pordidToListid.ToDictionary(i => i.Key, i => i.Value);
        }

        public async static Task<int> AddProductTypeToSet(int setid, int producttype_id)
        {
            var listitemid = 0;
            var quantity = 1;
            var listitem = await ListItems.GetItemInListForProductType(setid, producttype_id);
            if (listitem != null)
            {
                listitemid = listitem.id;
                quantity = listitem.quantity;
            }
            return await AddUpdateListItem(setid, listitemid, producttype_id, quantity);
        }

        public async static Task<int> AddUpdateListItem(int listid, int listitemid, int producttype_id, int quantity = 0)
        {
            var listitem = new v_listitems
            {
                list_id = listid
            };
            if (listitemid != 0)
            {
                listitem = await ListItems.GetListItem(listitemid);
            }
            listitem.producttype_id = producttype_id;

            listitem.quantity += quantity;
            if (listitem.quantity == 0)
            {
                listitem.quantity = 1;
            }

            var result = await ListItems.SaveListItems(new List<v_listitems>()
            {
                listitem
            });
            return result.ElementAtOrDefault(0);
        }
    }

    public class VListItem
    {
        public int Id { get; set; }
        public int ListId { get; set; }
        public int ProductTypeId { get; set; }
        public int Order { get; set; }
        public int Quantity { get; set; } = 1;
        public int Demarcation { get; set; }
        public bool SelectedForEdit { get; set; }
        public bool Confirm { get; set; }
        public int Original { get; set; }
        public bool? Checked { 
            get
            {
                return Demarcation == -1 ? null : Demarcation == 0 ? false : true;
            }
            set
            {
                Demarcation = value == null ? -1 : value == false ? 0 : 1;
            }
        }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public VProductType ProductType { get; set; }

        public async static Task<List<VListItem>> Init(int listid)
        {
            var items = await ListItems.GetListItems(listid);
            var listItems = items.Select(async i =>
            {
                var prodType = i.producttype_id > 0 ? await VProductType.Init(i.producttype_id) : new VProductType();
                var quantity = i.quantity == 0 ? 1 : i.quantity;
                return new VListItem
                {
                    Id = i.id,
                    ListId = i.list_id,
                    ProductTypeId = i.producttype_id,
                    Order = i.order,
                    Original = i.producttype_id,
                    Quantity = quantity,
                    Demarcation = i.demarcation,
                    CreatedDate = i.date_created,
                    UpdatedDate = i.date_updated,
                    ProductType = prodType
                };
            });
            var vItemsArray = await Task.WhenAll(listItems);
            var retList = vItemsArray.ToList();

            var locOrders = new Dictionary<int, int>();
            retList.ForEach(li =>
            {
                var index = li.ProductType?.LocationId ?? 0;
                li.Order = 0;
                if (!locOrders.ContainsKey(index))
                {
                    locOrders[index] = 0;
                }
                li.Order = locOrders[index];
                ++locOrders[index];
            });

            return retList;
        }

        public void UnLoadProductType()
        {
            ProductTypeId = 0;
            ProductType = new VProductType();
        }

        public static async Task<VListItem> GetListItemByUPC(int listid, string upc)
        {
            var item = await ListItems.GetListItemByUPC(listid, upc);
            if (item == null) return null;
            return new VListItem
            {
                Id = item.id,
                ListId = item.list_id,
                ProductTypeId = item.producttype_id,
                Order = item.order,
                Original = item.producttype_id,
                Quantity = item.quantity,
                Demarcation = item.demarcation,
                CreatedDate = item.date_created,
                UpdatedDate = item.date_updated,
                ProductType = await VProductType.Init(item.producttype_id) ?? new VProductType()
            };
        }

        public static async Task RemoveProductTypeFromAllLists(int productTypeId)
        {
            var listIds = new List<int>();
            var listItems = await ListItems.GetListItemsForProductType(productTypeId);
            listItems.ForEach(li => listIds.Add(li.list_id));
            await ListItems.DeleteListItems(listItems);
        }

        public static async Task MarkUnmarkListItems(int listid, int demarcation)
        {
            var list = await Init(listid);
            var tasks = list.Select(li => ListItems.GetListItem(li.Id));
            var listitems = await Task.WhenAll(tasks);
            listitems.ToList().ForEach(li => li.demarcation = demarcation);
            await ListItems.SaveListItems(listitems.ToList());
        }

        public static async Task MarkUnmarkListItemsForLocation(int listid, int locationId, int demarcation)
        {
            var list = await Init(listid);
            var tasks = list.Where(li => li.ProductType?.LocationId == locationId || (locationId == 0 && li.ProductTypeId == 0)).Select(li => ListItems.GetListItem(li.Id));
            var listitems = await Task.WhenAll(tasks);
            listitems.ToList().ForEach(li => li.demarcation = demarcation);
            await ListItems.SaveListItems(listitems.ToList());
        }

        public async static Task<int[]> CopyAllListItems(int listid, int newlistid)
        {
            var listitems = await ListItems.GetListItems(listid);
            var tasks = listitems.Select(li => CopyListItem(newlistid, li.id));
            return await Task.WhenAll(tasks);
        }

        public async static Task<int[]> CopyListItemsOrIncrement(int sourceListId, int destListid)
        {
            var sourceList = await ListItems.GetListItems(sourceListId);
            var sourceProdListIds = sourceList.Select(pt => pt.producttype_id).ToList();
            var sourceIncCounts = sourceList.Select(li => new KeyValuePair<int, int>(li.producttype_id, li.quantity == 0 ? 1 : li.quantity)).ToList();

            var destList = await ListItems.GetListItems(destListid);
            var destProdListIds = destList.Select(pt => pt.producttype_id).ToList();

            var destListItemIncr = destList.Where(li => sourceProdListIds.Contains(li.producttype_id)).ToList();
            var destIncCounts = destListItemIncr.Select(li => new KeyValuePair<int, int>(li.producttype_id, li.quantity == 0 ? 1 : li.quantity)).ToList();

            var combinedCounts = new List<KeyValuePair<int, int>>();
            combinedCounts.AddRange(sourceIncCounts);
            combinedCounts.AddRange(destIncCounts);

            var refinedCombined = new Dictionary<int, int>();
            combinedCounts.ForEach(li =>
            {
                if (refinedCombined.ContainsKey(li.Key))
                {
                    refinedCombined[li.Key] += li.Value;
                }
                else
                {
                    refinedCombined[li.Key] = li.Value;
                }
            });

            var adjustTasks = destListItemIncr.Select(li =>
            {
                if (refinedCombined.ContainsKey(li.producttype_id))
                {
                    return SetListItemQuantity(li.id, refinedCombined[li.producttype_id]);
                }
                return Task.Delay(0);
            });
            await Task.WhenAll(adjustTasks);

            var copyTasks = sourceList.Where(li => !destProdListIds.Contains(li.producttype_id)).Select(li => CopyListItem(destListid, li.id));
            return await Task.WhenAll(copyTasks);
        }

        public async static Task<int> CopyListItem(int newlistid, int listitemid)
        {
            var item = await ListItems.GetListItem(listitemid);
            item.id = 0;
            item.list_id = newlistid;
            item.demarcation = 0;
            return await ListItems.UpsertListItem(item);
        }

        public async static Task AdjustListItemQuantity(int listitemid, int upDn)
        {
            var item = await ListItems.GetListItem(listitemid);
            if (item == null) return;
            if (item.quantity == 0) item.quantity = 1;
            item.quantity = item.quantity + upDn;
            if (item.quantity < 0) item.quantity = 0;
            await ListItems.UpsertListItem(item);
        }

        public async static Task SetListItemQuantity(int listitemid, int quantity)
        { 
            await ListItems.SetListItemQuantity(listitemid, quantity);
        }

    }

    public class VProductType
    {
        public int Id { get; set; }
        public string TypeName { get; set; }
        public int ProductId { get; set; }
        public int LocationId { get; set; }
        public int PriceId { get; set; }
        public string Notes { get; set; }
        public string DetailsPreview => GetListViewLineDesc();
        public string SelectorName => TypeName + (!string.IsNullOrEmpty(DetailsPreview) ? " (" + DetailsPreview + ")" : null);
        public VProduct Product { get; set; }
        public VLocation Location { get; set; }
        public VPrice Price { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public async static Task<VProductType> Init(int producttype_id)
        {
            return await VProductTypeFactory(await ProductTypes.GetProductType(producttype_id));
        }

        public async static Task<VProductType> VProductTypeFactory(v_producttypes productType)
        {
            if (productType == null) return new VProductType();
            return new VProductType
            {
                Id = productType.id,
                ProductId = productType.product_id,
                LocationId = productType.location_id,
                PriceId = productType.price_id,
                Notes = System.Web.HttpUtility.HtmlDecode(productType.notes),
                TypeName = System.Web.HttpUtility.HtmlDecode(productType.typename),
                CreatedDate = productType.date_created,
                UpdatedDate = productType.date_updated,
                Product = await VProduct.Init(productType.product_id),
                Location = await VLocation.Init(productType.location_id),
                Price = await VPrice.Init(productType.price_id)
            };
        }

        public async static Task<List<VProductType>> GetUniqueProductTypeList(
            int listid,
            Support.Constants.SortTypes sortBy = Support.Constants.SortTypes.Product, 
            int sortDirection = 1
        ) {
            var dataList = await ProductTypes.GetProductTypesAvailableForList(listid);
            var tasks = await Task.WhenAll(dataList.Select(VProductTypeFactory));
            var retList = tasks.ToList();
            ApplySort(retList, sortBy, sortDirection);
            return retList;
        }

        public async static Task<List<VProductType>> GetProductTypeList(
            Support.Constants.SortTypes sortBy = Support.Constants.SortTypes.Product, 
            int sortDirection = 1, int limit = -1
        ) {
            var dataList = await ProductTypes.GetProductTypes(limit);
            var tasks = await Task.WhenAll(dataList.Select(VProductTypeFactory));
            var retList = tasks.ToList();
            ApplySort(retList, sortBy, sortDirection);
            return retList;
        }

        public static VProductType CloneProductType(VProductType pt)
        {
            return new VProductType
            {
                Id = pt.Id,
                TypeName = pt.TypeName,
                ProductId = pt.ProductId,
                LocationId = pt.LocationId,
                Notes = pt.Notes,
                PriceId = pt.PriceId,
                CreatedDate = pt.CreatedDate,
                UpdatedDate = pt.UpdatedDate
            };
        }

        public async static Task<VProductType> GetProductTypeById(int id)
        {
            return await VProductTypeFactory(await ProductTypes.GetProductType(id));
        }

        public async static Task<List<VProductType>> GetProductTypesByUPC(string upc)
        {
            var dataList = await ProductTypes.GetProducttypesForUPC(upc);
            var tasks = await Task.WhenAll(dataList.Select(VProductTypeFactory));
            var retList = tasks.ToList();
            return retList;
        }

        public async static Task<List<VProductType>> FilterProductTypes(string searchTerm, Support.Constants.SortTypes sortBy = Support.Constants.SortTypes.Product, int sortDirection = 1)
        {
            var dataList = await ProductTypes.FilterProductTypes(searchTerm.ToLower().Trim());
            var tasks = await Task.WhenAll(dataList.Select(VProductTypeFactory));
            var retList = tasks.ToList();
            ApplySort(retList, sortBy, sortDirection, searchTerm);
            return retList;
        }

        public static void ApplySort(List<VProductType> list, Support.Constants.SortTypes sortBy, int sortDirection, string weighted = null)
        {
            try
            {
                list.Sort((a, b) =>
                {
                    var sort = 0;
                    switch (sortBy)
                    {
                        case Support.Constants.SortTypes.Location:
                            if (a.Location != null && b.Location != null)
                                sort = (int)a.Location.Name?.CompareTo(b.Location.Name);
                            if (a.Location != null && b.Location == null)
                                return -1;
                            if (b.Location != null && a.Location == null)
                                return 1;
                            break;
                        case Support.Constants.SortTypes.Price:
                            if (a.Price != null && b.Price != null)
                                sort = a.Price.Price.CompareTo(b.Price.Price);
                            if (a.Price != null && b.Price == null)
                                return -1;
                            if (b.Price != null && a.Price == null)
                                return 1;
                            break;
                        case Support.Constants.SortTypes.Product:
                            break;
                    }
                    if(!string.IsNullOrEmpty(weighted) && !string.IsNullOrEmpty(weighted.Trim()))
                    {

                        Regex ar = new Regex(@"^" + weighted.Trim(), RegexOptions.IgnoreCase);
                        Regex br = new Regex(@"^" + weighted.Trim(), RegexOptions.IgnoreCase);

                        Match am = ar.Match(a.TypeName.Trim());
                        Match bm = br.Match(b.TypeName.Trim());

                        if (!am.Success && bm.Success)
                        {
                            return 1;
                        }
                        if (!bm.Success && am.Success)
                        {
                            return -1;
                        }
                        if (bm.Success && am.Success)
                        {
                            sort = am.Index.CompareTo(bm.Index);
                        }
                    }
                    if (sort == 0)
                    {
                        if (string.IsNullOrEmpty(a.TypeName))
                        {
                            return -1;
                        }
                        if (string.IsNullOrEmpty(b.TypeName))
                        {
                            return 1;
                        }
                        sort = a.TypeName.CompareTo(b.TypeName);
                    }
                    return sort * sortDirection;
                });
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public async static Task<int> SaveProductType(string typename, int producttypeid)
        {
            var pt = await GetProductTypeById(producttypeid);
            pt.TypeName = typename;
            return await SaveProductType(pt);
        }

        public async static Task<int> SaveProductType(string name)
        {
            return await SaveProductType(new VProductType
            {
                TypeName = name
            });
        }

        public async static Task<int> SaveProductType(VProductType prodObj)
        {
            var prodType = new v_producttypes
            {
                typename = prodObj.TypeName,
                notes = prodObj.Notes,
                product_id = prodObj.ProductId,
                location_id = prodObj.LocationId,
                price_id = prodObj.PriceId
            };
            if(prodObj.Id > 0)
            {
                prodType.id = prodObj.Id;
            }

            return await ProductTypes.UpsertProductType(prodType);
        }

        public async static Task<bool> DeleteProductType(int productTypeId)
        {
            await VListItem.RemoveProductTypeFromAllLists(productTypeId);
            await ProductTypes.DeleteProductType(await ProductTypes.GetProductType(productTypeId));
            return true;
        }

        public async static Task ClearProductTypeDetails(int priceId, int productId)
        {
            await ProductTypes.ClearProductTypeDetails(priceId, productId);
        }

        public static async Task<bool> RemoveLocationIdFromAllProductTypes(int locationid)
        {
            var productTypes = await ProductTypes.GetProductTypesForLocation(locationid);
            productTypes.ForEach(p => p.location_id = 0);
            await ProductTypes.UpsertProductTypes(productTypes);
            return true;
        }

        private string GetListViewLineDesc()
        {
            var brand = Product?.Brandname ?? string.Empty;
            var price = Price?.Price ?? 0;
            var location = Location?.Name ?? string.Empty;
            var retList = new List<string>();
            if (!string.IsNullOrEmpty(brand))
            {
                retList.Add(brand);
            }
            if (location != null)
            {
                retList.Add(location);
            }
            if (price > 0)
            {
                retList.Add("$" + price.ToString("0.00"));
            }
            if (retList.Count > 0)
            {
                return string.Join(", ", retList);
            }
            return null;
        }
    }

    public class VProduct
    {
        public int Id { get; set; }
        public string Brandname { get; set; }
        public string UPC { get; set; }
        public string Description { get; set; }
        public string FetchedName { get; set; }
        public string FetchedSize { get; set; }
        public string FetchedOwner { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public async static Task<VProduct> Init(int product_id)
        {
            if(product_id == 0) return null;
            var product = await Products.GetProduct(product_id);
            if(product == null) return null;
            return new VProduct
            {
                Id = product.id,
                Brandname = System.Web.HttpUtility.HtmlDecode(product.brandname),
                Description = System.Web.HttpUtility.HtmlDecode(product.description),
                UPC = product.upc,
                CreatedDate = product.date_created,
                UpdatedDate = product.date_updated
            };
        }

        public async static Task<VProduct> GetProductByUPC(string upc, int productid, bool forceFetch)
        {
            var product = await Products.GetProductByUPC(upc);
            if(product != null && !forceFetch)
            {
                return await Init(product.id);
            }

            var response = await ViandsService.FetchProductInfoByUPC(await LoginUtils.GetCurrentUserApiKey(), upc);
            if (response == null) return null;
            if (response.status != "ok") return null;

            var newProducts = new VProduct
            {
                FetchedName = Utils.ToTitleCase(response.title),
                Description = Utils.CapitalizeSentence(response.description),
                Brandname = response.brand,
                UPC = response.upc,
                FetchedSize = response.size,
                FetchedOwner = response.ownerid
            };

            if (productid > 0)
            {
                newProducts.Id = productid;
            }

            newProducts.Id = await SaveProduct(newProducts);
            return newProducts;
        }

        public async static Task<int> SaveProduct(VProduct product)
        {
            var prodInfo = new v_products
            {
                brandname = product.Brandname,
                upc = product.UPC,
                description = product.Description
            };
            if(product.Id > 0)
            {
                prodInfo.id = product.Id;
            }

            var validator = new v_products();

            var notValid = prodInfo.brandname == validator.brandname &&
                prodInfo.upc == validator.upc &&
                prodInfo.description == validator.description;

            if (notValid) return 0;

            return await Products.UpsertProduct(prodInfo);
        }
    }

    public class VLocation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string Coordinates { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public int ItemCount { get; set; }
        public bool Collapsed { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public async static Task<VLocation> Init(int location_id)
        {
            if (location_id == 0) return null;
            var location = await Locations.GetLocation(location_id);
            return new VLocation
            {
                Id = location.id,
                Name = System.Web.HttpUtility.HtmlDecode(location.name),
                Address = System.Web.HttpUtility.HtmlDecode(location.address),
                City = System.Web.HttpUtility.HtmlDecode(location.city),
                Zip = System.Web.HttpUtility.HtmlDecode(location.zip),
                Description = System.Web.HttpUtility.HtmlDecode(location.description),
                Logo = location.logo,
                Order = location.order,
                Coordinates = location.coordinates,
                CreatedDate = location.date_created,
                UpdatedDate = location.date_updated
            };
        }

        public async static Task<List<VLocation>> GetAllLocations()
        {
            var locdata = await Locations.GetLocations();
            var locentries = locdata.Select(l => Init(l.id));
            var tasks = await Task.WhenAll(locentries);
            return tasks.OrderBy(l => l.Order).ToList();
        }

        public async static Task<List<VLocation>> FilterAllLocations(string searchTerm)
        {
            var locdata = await Locations.FilterLocations(searchTerm);
            var locentries = locdata.Select(l => Init(l.id));
            var tasks = await Task.WhenAll(locentries);
            return tasks.OrderBy(l => l.Order).ToList();
        }

        public async static Task<List<VLocation>> GetAllLocationsForLists(int listid)
        {
            var retList = await GetAllLocations();
            retList.Insert(0, new VLocation
            {
                Id = 0,
                Order = 0
            });
            await Task.WhenAll(
                retList.Select(async l =>
                {
                    var li = await VListItem.Init(listid);
                    l.ItemCount = li?.Where(i => i.ProductType?.LocationId == l.Id).Count() ?? 0;
                })
            );
            return retList;
        }

        public async static Task<bool> DeleteLocation(int locationid)
        {
            await VProductType.RemoveLocationIdFromAllProductTypes(locationid);
            await Locations.DeleteLocation(await Locations.GetLocation(locationid));
            return true;
        }

        public async static Task SaveLocations(List<VLocation> list)
        {
            await Locations.SaveLocations(list.Select(i => new v_locations
            {
                id = i.Id,
                name = i.Name,
                address = i.Address,
                city = i.City,
                zip = i.Zip,
                description = i.Description,
                coordinates = i.Coordinates,
                logo = i.Logo,
                order = i.Order,
                date_created = i.CreatedDate, 
                date_updated = i.UpdatedDate
            }).ToList());
        }

        public async static Task<int> SaveLocation(VLocation location)
        {
            var v_location = new v_locations
            {
                name = location.Name,
                address = location.Address,
                city = location.City,
                zip = location.Zip,
                description = location.Description,
                coordinates = location.Coordinates,
                order = location.Order,
                logo = location.Logo,
                date_created = location.CreatedDate,
                date_updated = location.UpdatedDate
            };
            if(location.Id != 0)
            {
                v_location.id = location.Id;
            }

            return await Locations.UpsertLocation(v_location);
        }
    }

    public class VPrice
    {
        public int Id { get; set; }
        public string ProductSize { get; set; }
        public decimal Price { get; set; }
        public string DisplayPrice => GetDisplayPrice();
        public string PerUnitType { 
            get; 
            set; 
        }
        public bool OnSale { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public async static Task<VPrice> Init(int price_id)
        {
            if (price_id == 0) return null;
            var price = await Prices.GetPrice(price_id);
            if (price == null) return null;
            return new VPrice
            {
                Id = price.id,
                ProductSize = System.Web.HttpUtility.HtmlDecode(price.product_size),
                PerUnitType = System.Web.HttpUtility.HtmlDecode(price.per_unit_type),
                Price = Convert.ToDecimal(price.price) / 100,
                OnSale = price.on_sale,
                CreatedDate = price.date_created,
                UpdatedDate = price.date_updated
            };
        }

        public async static Task<int> SavePrice(VPrice priceObj)
        {
            if (priceObj == null)
            {
                priceObj = new VPrice();
            }
            var priceInfo = new v_prices
            {
                price = (int)(priceObj.Price * 100),
                per_unit_type = priceObj.PerUnitType,
                product_size = priceObj.ProductSize,
                on_sale = priceObj.OnSale
            };
            if (priceObj.Id > 0)
            {
                priceInfo.id = priceObj.Id;
            }

            var validator = new v_prices();
            var notValid = priceInfo.price == validator.price &&
                priceInfo.per_unit_type == validator.per_unit_type &&
                priceInfo.product_size == validator.product_size &&
                priceInfo.on_sale == validator.on_sale;

            if (notValid) return 0;

            return await Prices.UpsertPrice(priceInfo);
        }

        public string GetDisplayPrice()
        {
            if (Price <= 0) return null;
            return Price.ToString("0.00");
        }
    }
}
