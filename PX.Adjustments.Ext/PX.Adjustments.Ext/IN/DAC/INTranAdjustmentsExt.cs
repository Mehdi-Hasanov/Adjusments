using PX.Data;
using System;
using PX.Objects.IN;
using PX.Objects.CS;

namespace PX.Adjustments.Ext
{
    public sealed class INTranAdjustmentsExt : PXCacheExtension<PX.Objects.IN.INTran>
    {
        public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.inventory>();


        #region UsrCurrentQtyOnHand
        public abstract class usrCurrentQtyOnHand : PX.Data.BQL.BqlDecimal.Field<usrCurrentQtyOnHand> { }

        [PXDBQuantity]
        [PXUIField(DisplayName = "Current Qty. On Hand", Enabled =false)]
        public Decimal? UsrCurrentQtyOnHand { get; set; }
        #endregion

        #region UsrAdjustedQtyOnHand
        public abstract class usrAdjustedQtyOnHand : PX.Data.BQL.BqlDecimal.Field<usrAdjustedQtyOnHand> { }

        [PXQuantity]
        [PXUIField(DisplayName = "Adjusted Qty. On Hand", Enabled = false)]
        public Decimal? UsrAdjustedQtyOnHand { get; set; }
        #endregion

        #region UsrCurrentTranCost
        public abstract class usrCurrentTranCost : PX.Data.BQL.BqlDecimal.Field<usrCurrentTranCost> { }

        [PXDBPriceCost]
        [PXUIField(DisplayName = "Current Ext. Cost", Enabled = false)]
        public Decimal? UsrCurrentTranCost { get; set; }
        #endregion

        #region UsrAdjustedTranCost
        public abstract class usrAdjustedTranCost : PX.Data.BQL.BqlDecimal.Field<usrAdjustedTranCost> { }

        [PXPriceCost]
        [PXUIField(DisplayName = "Adjusted Ext. Cost", Enabled = false)]
        public Decimal? UsrAdjustedTranCost { get; set; }
        #endregion
    }
}