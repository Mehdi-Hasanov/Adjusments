using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Adjustments.Ext
{
    public class INReleaseProcessPXExt : PXGraphExtension<INReleaseProcess>
    {
        public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.inventory>();

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
                INLotSerialStatus status = PXSelectReadonly<INLotSerialStatus, Where<INLotSerialStatus.inventoryID, Equal<Required<INTran.inventoryID>>, And<INLotSerialStatus.siteID, Equal<Required<INTran.siteID>>, And<INLotSerialStatus.locationID, Equal<Required<INTran.locationID>>, And<INLotSerialStatus.lotSerialNbr, Equal<Required<INTran.lotSerialNbr>>>>>>>.SelectSingleBound(Base, null, tran.InventoryID, tran.SiteID, tran.LocationID, tran.LotSerialNbr);
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

        protected virtual void _(Events.FieldUpdated<INTran, INTran.released> args)
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

        #endregion

    }
}