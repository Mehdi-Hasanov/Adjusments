using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.Common;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN.Services;
using PX.Objects;
using PX.Objects.IN;

namespace PX.Objects.IN
{
  public class INAdjustmentEntry_Extension : PXGraphExtension<PX.Objects.IN.INAdjustmentEntry>
  {
		decimal? adjustedCost;
		decimal? adjustedQty;

		public decimal? DefaultCurrentTotalCost(PXCache cache, INTran tran, bool setZero = false)
		{
			if ((Base.adjustment.Current != null && Base.adjustment.Current.OrigModule == INRegister.origModule.PI)
				|| tran?.InventoryID == null)
				return null;

			object TotalCost = null;

			InventoryItem item = InventoryItem.PK.Find(Base, tran?.InventoryID);

			if (item.ValMethod != INValMethod.FIFO)//item.ValMethod == INValMethod.Specific && string.IsNullOrEmpty(tran.LotSerialNbr) == false)
			{
				INCostStatus status =
					SelectFrom<INCostStatus>.
					LeftJoin<INLocation>.On<INLocation.locationID.IsEqual<INTran.locationID.FromCurrent>>.
					InnerJoin<INCostSubItemXRef>.On<INCostSubItemXRef.costSubItemID.IsEqual<INCostStatus.costSubItemID>>.
					Where<
						INCostStatus.inventoryID.IsEqual<INTran.inventoryID.FromCurrent>.
						And<INCostSubItemXRef.subItemID.IsEqual<INTran.subItemID.FromCurrent>>.
						//And<INCostStatus.lotSerialNbr.IsEqual<INTran.lotSerialNbr.FromCurrent>>.
						And<
							INLocation.isCosted.IsEqual<False>.
							And<INCostStatus.costSiteID.IsEqual<INTran.siteID.FromCurrent>>.
							Or<INCostStatus.costSiteID.IsEqual<INTran.locationID.FromCurrent>>>>.
					View.SelectSingleBound(Base, new object[] { tran });
				if (status != null && status.QtyOnHand != 0m)
				{
					TotalCost = PXDBPriceCostAttribute.Round((decimal)(status.TotalCost));
				}
			}
			else if (item.ValMethod == INValMethod.FIFO && string.IsNullOrEmpty(tran.OrigRefNbr) == false)
			{
				INCostStatus status =
					SelectFrom<INCostStatus>.
					LeftJoin<INLocation>.On<INLocation.locationID.IsEqual<INTran.locationID.FromCurrent>>.
					InnerJoin<INCostSubItemXRef>.On<INCostSubItemXRef.costSubItemID.IsEqual<INCostStatus.costSubItemID>>.
					Where<
						INCostStatus.inventoryID.IsEqual<INTran.inventoryID.FromCurrent>.
						And<INCostSubItemXRef.subItemID.IsEqual<INTran.subItemID.FromCurrent>>.
						And<INCostStatus.receiptNbr.IsEqual<INTran.origRefNbr.FromCurrent>>.
						And<
							INLocation.isCosted.IsEqual<False>.
							And<INCostStatus.costSiteID.IsEqual<INTran.siteID.FromCurrent>>.
							Or<INCostStatus.costSiteID.IsEqual<INTran.locationID.FromCurrent>>>>.
					View.SelectSingleBound(Base, new object[] { tran });
				if (status != null && status.QtyOnHand != 0m)
				{
					TotalCost = PXDBPriceCostAttribute.Round((decimal)(status.TotalCost));
				}
			}
			else
			{
    //            if (item.ValMethod == INValMethod.Average)
    //            {
    //                cache.RaiseFieldDefaulting<INTran.avgCost>(tran, out TotalCost);
				//	TotalCost = (decimal)TotalCost * tran.Qty;

				//}
                //if (UnitCost == null || (decimal)UnitCost == 0m)
                //{
                //	cache.RaiseFieldDefaulting<INTran.unitCost>(tran, out UnitCost);
                //}
            }


			decimal? qty = (decimal?)cache.GetValue<INTran.qty>(tran);

			if (TotalCost != null && ((decimal)TotalCost != 0m || setZero || qty < 0m))
			{
				if ((decimal)TotalCost < 0m)
					cache.RaiseFieldDefaulting<INTranExt.usrCurrentTranCost>(tran, out TotalCost);

				decimal? totalcost = INUnitAttribute.ConvertToBase<INTran.inventoryID>(cache, tran, tran.UOM, (decimal)TotalCost, INPrecision.UNITCOST);

				//suppress trancost recalculation for cost only adjustments
				//if (qty == 0m)
				//{
				//	cache.SetValue<INTran.unitCost>(tran, unitcost);
				//}
				//else
				//{
				//	cache.SetValueExt<INTran.unitCost>(tran, unitcost);
				//}
				return totalcost;

			}
			return null;
		}

		public virtual void _(Events.FieldDefaulting<INTran, INTranExt.usrCurrentTranCost> e)
		{
			if (e.Cache == null || e.Row == null || e.Row.Released.GetValueOrDefault(true)
				|| e.Row.InventoryID == null || e.Row.SiteID == null) return;
			decimal currentCost = DefaultCurrentTotalCost(e.Cache, e.Row) ?? 0m;
			e.NewValue = currentCost;
			adjustedCost = currentCost + e.Row.TranCost;
		}
		public virtual void _(Events.FieldSelecting<INTran, INTranExt.usrCurrentTranCost> args)
		{
			if (args.Cache == null || args.Row == null || args.Row.Released.GetValueOrDefault(true)
				|| args.Row.InventoryID == null || args.Row.SiteID == null) return;
			var row = args.Row;
			args.Cache.SetDefaultExt<INTranExt.usrCurrentTranCost>(args.Row);
		}

		public virtual void _(Events.FieldSelecting<INTran, INTranExt.usrAdjustedTranCost> args)
		{
			if (args.Cache == null || args.Row == null || args.Row.Released.GetValueOrDefault(true)
				|| args.Row.InventoryID == null || args.Row.SiteID == null) return;
			var row = args.Row;
			args.ReturnValue = adjustedCost;
		}

		public virtual void _(Events.FieldDefaulting<INTran, INTranExt.usrCurrentQtyOnHand> e)
		{
			if (e.Cache == null || e.Row == null || e.Row.Released.GetValueOrDefault(true)
				|| e.Row.InventoryID == null || e.Row.SiteID == null) return;

			decimal? totalQty;
			if (e.Row.LotSerialNbr != null && e.Row.LotSerialNbr.Length > 0)
			{
				INLotSerialStatus status = PXSelect<INLotSerialStatus, Where<INLotSerialStatus.inventoryID, Equal<Current<INTran.inventoryID>>, And<INLotSerialStatus.siteID, Equal<Current<INTran.siteID>>, And<INLotSerialStatus.lotSerialNbr, Equal<Current<INTran.lotSerialNbr>>>>>>.SelectSingleBound(Base, null);
				totalQty = status.QtyOnHand.GetValueOrDefault(0m);
			}
			else if (e.Row.LocationID.HasValue)
            {
				INLocationStatus status = PXSelect<INLocationStatus, Where<INLocationStatus.inventoryID, Equal<Current<INTran.inventoryID>>, And<INLocationStatus.siteID, Equal<Current<INTran.siteID>>, And<INLocationStatus.locationID, Equal<Current<INTran.locationID>>>>>>.SelectSingleBound(Base, null);
				totalQty = status.QtyOnHand.GetValueOrDefault(0m);
			}
			else
            {
				INSiteStatus status = PXSelect<INSiteStatus, Where<INSiteStatus.inventoryID, Equal<Current<INTran.inventoryID>>, And<INSiteStatus.siteID, Equal<Current<INTran.siteID>>>>>.SelectSingleBound(Base, null);
				totalQty = status.QtyOnHand.GetValueOrDefault(0m);
			}
			e.NewValue = totalQty;

			adjustedQty = totalQty + e.Row.Qty;

		}

		public virtual void _(Events.FieldSelecting<INTran, INTranExt.usrCurrentQtyOnHand> args)
		{
			if (args.Cache == null || args.Row == null || args.Row.Released.GetValueOrDefault(true)
				|| args.Row.InventoryID == null || args.Row.SiteID == null) return;
			args.Cache.SetDefaultExt<INTranExt.usrCurrentQtyOnHand>(args.Row);
		}

		public virtual void _(Events.FieldSelecting<INTran, INTranExt.usrAdjustedQtyOnHand> args)
		{
			if (args.Cache == null || args.Row == null || args.Row.Released.GetValueOrDefault(true)
				|| args.Row.InventoryID == null || args.Row.SiteID == null) return;
			var row = args.Row;
			args.ReturnValue = adjustedQty;
		}
	}
}