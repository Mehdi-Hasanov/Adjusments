using PX.Data;
using System;
using System.Collections;
using static PX.Objects.IN.INTranExt;

namespace PX.Objects.IN
{
    [PXProtectedAccess]
    public class INAdjustmentEntry_Extension : PXGraphExtension<PX.Objects.IN.INAdjustmentEntry>
    {

        #region Helper Methods

        public decimal? getCurrentTranCost(INTran tran)
        {
            if (!String.IsNullOrEmpty(tran.LotSerialNbr))
            {
                INCostStatus lsData = PXSelectReadonly<INCostStatus, Where<INCostStatus.inventoryID, Equal<Required<INCostStatus.inventoryID>>,
                                                                    And<INCostStatus.siteID, Equal<Required<INCostStatus.siteID>>,
                                                                    And<INCostStatus.lotSerialNbr, Equal<Required<INCostStatus.lotSerialNbr>>>>>>.
                                                                    SelectWindowed(Base, 0, 1, tran.InventoryID, tran.SiteID, tran.LotSerialNbr);
                return lsData?.TotalCost.GetValueOrDefault(0m) ?? 0m;
            }
            INItemSite data = PXSelectReadonly<INItemSite, Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
                                                    And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>>
                                                    .SelectWindowed(Base, 0, 1, tran.InventoryID, tran.SiteID);
            INTranExt tranExt = tran.GetExtension<INTranExt>();
            return data?.AvgCost.GetValueOrDefault(0m) ?? 0m * tranExt?.UsrCurrentQtyOnHand.GetValueOrDefault(0m) ?? 0m;

        }

        public decimal? getCurrentQtyOnHand(INTran tran)
        {
            if (tran.LotSerialNbr != null && tran.LotSerialNbr.Length > 0)
            {
                INLotSerialStatus status = PXSelect<INLotSerialStatus, Where<INLotSerialStatus.inventoryID, Equal<Required<INTran.inventoryID>>, And<INLotSerialStatus.siteID, Equal<Required<INTran.siteID>>, And<INLotSerialStatus.lotSerialNbr, Equal<Required<INTran.lotSerialNbr>>>>>>.SelectSingleBound(Base, null, tran.InventoryID, tran.SiteID, tran.LotSerialNbr);
                return status?.QtyOnHand.GetValueOrDefault(0m) ?? 0m;
            }
            else if (tran.LocationID.HasValue)
            {
                INLocationStatus status = PXSelect<INLocationStatus, Where<INLocationStatus.inventoryID, Equal<Required<INTran.inventoryID>>, And<INLocationStatus.siteID, Equal<Required<INTran.siteID>>, And<INLocationStatus.locationID, Equal<Required<INTran.locationID>>>>>>.SelectSingleBound(Base, null, tran.InventoryID, tran.SiteID, tran.LocationID);
                return status?.QtyOnHand.GetValueOrDefault(0m) ?? 0m;
            }
            else
            {
                INSiteStatus status = PXSelect<INSiteStatus, Where<INSiteStatus.inventoryID, Equal<Required<INTran.inventoryID>>, And<INSiteStatus.siteID, Equal<Required<INTran.siteID>>>>>.SelectSingleBound(Base, null, tran.InventoryID, tran.SiteID);
                return status?.QtyOnHand.GetValueOrDefault(0m) ?? 0m;
            }
        }
        
        #endregion

        #region Overrides
        public delegate IEnumerable ReleaseDelegate(PXAdapter adapter);
        [PXOverride]
        public IEnumerable Release(PXAdapter adapter, ReleaseDelegate BaseMethod)
        {
            PXCache cache = Base.transactions.Cache;
            foreach (INTran item in Base.transactions.Select())
            {
                Base.Caches[typeof(INTran)].SetValue<usrCurrentQtyOnHand>(item, getCurrentQtyOnHand(item));
                Base.Caches[typeof(INTran)].SetValue<usrCurrentTranCost>(item, getCurrentTranCost(item));
                cache.Update(item);
                Base.Persist();
            }
            return BaseMethod.Invoke(adapter);
        }
        #endregion

        #region Event Handlers

        public virtual void _(Events.RowSelecting<INTran> args)
        {
            using (new PXConnectionScope())
            {
                if (args.Cache == null || args.Row == null || args.Row.InventoryID == null
                    || args.Row.SiteID == null) return;
                INTran row = args.Row;
                INTranExt rowExt = row.GetExtension<INTranExt>();
                if (!args.Row.Released.GetValueOrDefault(false))
                {
                    rowExt.UsrCurrentQtyOnHand = getCurrentQtyOnHand(row);
                    rowExt.UsrCurrentTranCost = getCurrentTranCost(row);
                }
                rowExt.UsrAdjustedQtyOnHand = rowExt.UsrCurrentQtyOnHand + row.Qty;
                rowExt.UsrAdjustedTranCost = rowExt.UsrCurrentTranCost + row.TranCost;
            }
        }

        public virtual void _(Events.RowSelected<INRegister> args)
        {
            if (args.Cache == null || args.Row == null || !args.Row.Released.GetValueOrDefault(false)) return;
            PXUIFieldAttribute.SetDisplayName<INTranExt.usrCurrentQtyOnHand>(args.Cache, "Original Qty. On Hand");
            PXUIFieldAttribute.SetDisplayName<INTranExt.usrCurrentTranCost>(args.Cache, "Original Ext. Cost");
        }

        protected virtual void _(Events.FieldUpdated<INTran, INTran.inventoryID> args)
        {
            if (args.Cache == null || args.Row == null || args.Row.InventoryID == null
                || args.Row.SiteID == null) return;
            INTran row = args.Row;
            INTranExt rowExt = row.GetExtension<INTranExt>();
            rowExt.UsrCurrentQtyOnHand = getCurrentQtyOnHand(row);
            rowExt.UsrAdjustedQtyOnHand = rowExt.UsrCurrentQtyOnHand + row.Qty;

            rowExt.UsrCurrentTranCost = getCurrentTranCost(row);
            rowExt.UsrAdjustedTranCost = rowExt.UsrCurrentTranCost + row.TranCost;
        }

        protected virtual void _(Events.FieldUpdated<INTran, INTran.siteID> args)
        {
            if (args.Cache == null || args.Row == null || args.Row.InventoryID == null
                || args.Row.SiteID == null) return;
            INTran row = args.Row;
            INTranExt rowExt = row.GetExtension<INTranExt>();
            rowExt.UsrCurrentQtyOnHand = getCurrentQtyOnHand(row);
            rowExt.UsrAdjustedQtyOnHand = rowExt.UsrCurrentQtyOnHand + row.Qty;

            rowExt.UsrCurrentTranCost = getCurrentTranCost(row);
            rowExt.UsrAdjustedTranCost = rowExt.UsrCurrentTranCost + row.TranCost;
        }

        protected virtual void _(Events.FieldUpdated<INTran, INTran.locationID> args)
        {
            if (args.Cache == null || args.Row == null || args.Row.InventoryID == null
                || args.Row.SiteID == null) return;
            INTran row = args.Row;
            INTranExt rowExt = row.GetExtension<INTranExt>();
            rowExt.UsrCurrentQtyOnHand = getCurrentQtyOnHand(row);
            rowExt.UsrAdjustedQtyOnHand = rowExt.UsrCurrentQtyOnHand + row.Qty;

            rowExt.UsrCurrentTranCost = getCurrentTranCost(row);
            rowExt.UsrAdjustedTranCost = rowExt.UsrCurrentTranCost + row.TranCost;
        }

        protected virtual void _(Events.FieldUpdated<INTran, INTran.qty> args)
        {
            if (args.Cache == null || args.Row == null || args.Row.InventoryID == null
                || args.Row.SiteID == null) return;
            INTran row = args.Row;
            INTranExt rowExt = row.GetExtension<INTranExt>();
            rowExt.UsrAdjustedQtyOnHand = rowExt.UsrCurrentQtyOnHand + row.Qty;
        }

        protected virtual void _(Events.FieldUpdated<INTran, INTran.tranCost> args)
        {
            if (args.Cache == null || args.Row == null || args.Row.InventoryID == null
                || args.Row.SiteID == null) return;
            INTran row = args.Row;
            INTranExt rowExt = row.GetExtension<INTranExt>();
            rowExt.UsrAdjustedTranCost = rowExt.UsrCurrentTranCost + row.TranCost;
        }

        #endregion

    }
}