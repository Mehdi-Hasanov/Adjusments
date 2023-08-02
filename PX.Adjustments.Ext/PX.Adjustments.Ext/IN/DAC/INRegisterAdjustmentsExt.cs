using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using System;

namespace PX.Adjustments.Ext
{
    public sealed class INRegisterAdjustmentsExt : PXCacheExtension<PX.Objects.IN.INRegister>
    {
        public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.inventory>();


        #region UsrCurrentTotalQty
        public abstract class usrCurrentTotalQty : PX.Data.BQL.BqlDecimal.Field<usrCurrentTotalQty> { }

        [PXQuantity]
        [PXUIField(DisplayName = "Current Total Qty.", Enabled =false)]
        public Decimal? UsrCurrentTotalQty { get; set; }
        #endregion

        #region UsrAdjustedTotalQty
        public abstract class usrAdjustedTotalQty : PX.Data.BQL.BqlDecimal.Field<usrAdjustedTotalQty> { }

        [PXQuantity]
        [PXUIField(DisplayName = "Adjusted Total Qty.", Enabled = false)]
        public Decimal? UsrAdjustedTotalQty { get; set; }
        #endregion

        #region UsrCurrentTotalTranCost
        public abstract class usrCurrentTotalTranCost : PX.Data.BQL.BqlDecimal.Field<usrCurrentTotalTranCost> { }

        [PXPriceCost]
        [PXUIField(DisplayName = "Current Total Cost", Enabled = false)]
        public Decimal? UsrCurrentTotalTranCost { get; set; }
        #endregion

        #region UsrAdjustedTotalTranCost
        public abstract class usrAdjustedTotalTranCost : PX.Data.BQL.BqlDecimal.Field<usrAdjustedTotalTranCost> { }

        [PXPriceCost]
        [PXUIField(DisplayName = "Adjusted Total Cost", Enabled = false)]
        public Decimal? UsrAdjustedTotalTranCost { get; set; }
        #endregion
    }
}