using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using System;
using static PX.Data.BQL.BqlPlaceholder;

namespace PX.Adjustments.Ext
{
    public class INAdjustmentEntryPXExt : PXGraphExtension<PX.Objects.IN.INAdjustmentEntry>
    {
        public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.inventory>();

        private decimal? currentTotalQty;
        private decimal? currentTotalCost;

        #region Helper Methods

        public decimal? getCurrentTranCost(INTran tran)
        {
            decimal? returnValue;
            INTranAdjustmentsExt tranExt = tran.GetExtension<INTranAdjustmentsExt>();

            INItemSite data = PXSelectReadonly<INItemSite, Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
                                    And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>>
                                    .SelectWindowed(Base, 0, 1, tran.InventoryID, tran.SiteID);
            returnValue = data?.AvgCost.GetValueOrDefault(0m) * tranExt?.UsrCurrentQtyOnHand.GetValueOrDefault(0m);

            return returnValue ?? 0m;
        }

        public decimal? getCurrentQtyOnHand(INTran tran)
        {
            decimal? returnValue;
            InventoryItem inventoryItem = InventoryItem.PK.Find(Base, tran.InventoryID);
            InventoryItemAdjustmentsExt inventoryItemExt = inventoryItem.GetExtension<InventoryItemAdjustmentsExt>();
            if (tran.LotSerialNbr != null && tran.LotSerialNbr.Length > 0 
                && inventoryItemExt?.LotSerTrack != INLotSerTrack.SerialNumbered
                && inventoryItemExt?.LotSerTrack != INLotSerTrack.NotNumbered)
            {
                INLotSerialStatus status = PXSelectReadonly<INLotSerialStatus, Where<INLotSerialStatus.inventoryID, Equal<Required<INTran.inventoryID>>, And<INLotSerialStatus.siteID, Equal<Required<INTran.siteID>>, And<INLotSerialStatus.locationID, Equal<Required<INTran.locationID>>,And<INLotSerialStatus.lotSerialNbr, Equal<Required<INTran.lotSerialNbr>>>>>>>.SelectSingleBound(Base, null, tran.InventoryID, tran.SiteID, tran.LocationID, tran.LotSerialNbr);
                returnValue = status?.QtyOnHand.GetValueOrDefault(0m);
            }
            else if (tran.LocationID.HasValue)
            {
                INLocationStatus status = PXSelectReadonly<INLocationStatus, Where<INLocationStatus.inventoryID, Equal<Required<INTran.inventoryID>>, And<INLocationStatus.siteID, Equal<Required<INTran.siteID>>, And<INLocationStatus.locationID, Equal<Required<INTran.locationID>>>>>>.SelectSingleBound(Base, null, tran.InventoryID, tran.SiteID, tran.LocationID);
                returnValue = status?.QtyOnHand.GetValueOrDefault(0m);
            }
            else
            {
                INSiteStatus status = PXSelectReadonly<INSiteStatus, Where<INSiteStatus.inventoryID, Equal<Required<INTran.inventoryID>>, And<INSiteStatus.siteID, Equal<Required<INTran.siteID>>>>>.SelectSingleBound(Base, null, tran.InventoryID, tran.SiteID);
                returnValue = status?.QtyOnHand.GetValueOrDefault(0m);
            }
            return returnValue ?? 0m;
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
                INTranAdjustmentsExt rowExt = row.GetExtension<INTranAdjustmentsExt>();
                if (row?.Released != true)
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
            if (args.Cache == null || args.Row == null || args.Row?.Released != true) return;
            PXUIFieldAttribute.SetDisplayName<INTranAdjustmentsExt.usrCurrentQtyOnHand>(Base.transactions.Cache, "Original Qty. On Hand");
            PXUIFieldAttribute.SetDisplayName<INTranAdjustmentsExt.usrCurrentTranCost>(Base.transactions.Cache, "Original Ext. Cost");
        }

        public virtual void _(Events.FieldSelecting<INRegister, INRegisterAdjustmentsExt.usrCurrentTotalQty> args)
        {
            if (args.Cache == null || args.Row == null) return;
            INRegister row = (INRegister)args.Row;
            decimal? returnValue = 0;
            foreach (INTran tran in Base.transactions.Select())
            {
                var tranExt = tran.GetExtension<INTranAdjustmentsExt>();
                returnValue += tranExt.UsrCurrentQtyOnHand;
            }
            args.ReturnValue = returnValue;
            currentTotalQty = returnValue;
        }

        public virtual void _(Events.FieldSelecting<INRegister, INRegisterAdjustmentsExt.usrCurrentTotalTranCost> args)
        {
            if (args.Cache == null || args.Row == null) return;
            INRegister row = (INRegister)args.Row;
            decimal? returnValue = 0;
            foreach (INTran tran in Base.transactions.Select())
            {
                var tranExt = tran.GetExtension<INTranAdjustmentsExt>();
                returnValue += tranExt.UsrCurrentTranCost;
            }
            args.ReturnValue = returnValue;
            currentTotalCost = returnValue;
        }

        public virtual void _(Events.FieldSelecting<INRegister, INRegisterAdjustmentsExt.usrAdjustedTotalQty> args)
        {
            if (args.Cache == null || args.Row == null) return;
            INRegister row = (INRegister)args.Row;
            args.ReturnValue = currentTotalQty + row.TotalQty;
        }

        public virtual void _(Events.FieldSelecting<INRegister, INRegisterAdjustmentsExt.usrAdjustedTotalTranCost> args)
        {
            if (args.Cache == null || args.Row == null) return;
            INRegister row = (INRegister)args.Row;
            args.ReturnValue = currentTotalCost + row.TotalCost;
        }

        protected virtual void _(Events.FieldUpdated<INTran, INTran.inventoryID> args)
        {
            if (args.Cache == null || args.Row == null || args.Row.InventoryID == null
                || args.Row.SiteID == null) return;
            INTran row = args.Row;
            row.LotSerialNbr = null;
            INTranAdjustmentsExt rowExt = row.GetExtension<INTranAdjustmentsExt>();
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
            INTranAdjustmentsExt rowExt = row.GetExtension<INTranAdjustmentsExt>();
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
            INTranAdjustmentsExt rowExt = row.GetExtension<INTranAdjustmentsExt>();
            rowExt.UsrCurrentQtyOnHand = getCurrentQtyOnHand(row);
            rowExt.UsrAdjustedQtyOnHand = rowExt.UsrCurrentQtyOnHand + row.Qty;

            rowExt.UsrCurrentTranCost = getCurrentTranCost(row);
            rowExt.UsrAdjustedTranCost = rowExt.UsrCurrentTranCost + row.TranCost;
        }

        protected virtual void _(Events.FieldUpdated<INTran, INTran.lotSerialNbr> args)
        {
            if (args.Cache == null || args.Row == null || args.Row.InventoryID == null
                || args.Row.SiteID == null || String.IsNullOrEmpty(args.Row.LotSerialNbr)) return;
            INTran row = args.Row;
            INTranAdjustmentsExt rowExt = row.GetExtension<INTranAdjustmentsExt>();
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
            INTranAdjustmentsExt rowExt = row.GetExtension<INTranAdjustmentsExt>();
            rowExt.UsrAdjustedQtyOnHand = rowExt.UsrCurrentQtyOnHand + row.Qty;
        }

        protected virtual void _(Events.FieldUpdated<INTran, INTran.tranCost> args)
        {
            if (args.Cache == null || args.Row == null || args.Row.InventoryID == null
                || args.Row.SiteID == null) return;
            INTran row = args.Row;
            INTranAdjustmentsExt rowExt = row.GetExtension<INTranAdjustmentsExt>();
            rowExt.UsrAdjustedTranCost = rowExt.UsrCurrentTranCost + row.TranCost;
        }

        #endregion

    }
}