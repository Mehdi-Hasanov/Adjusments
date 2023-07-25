using PX.Data;
using System;

namespace PX.Objects.IN
{
    public class INRegisterAdjustmentsExt : PXCacheExtension<PX.Objects.IN.INRegister>
    {
        #region UsrCurrentTotalQty
        public abstract class usrCurrentTotalQty : PX.Data.BQL.BqlDecimal.Field<usrCurrentTotalQty> { }

        [PXQuantity]
        [PXUIField(DisplayName = "Current Total Qty.", Enabled =false)]
        public virtual Decimal? UsrCurrentTotalQty { get; set; }
        #endregion

        #region UsrAdjustedTotalQty
        public abstract class usrAdjustedTotalQty : PX.Data.BQL.BqlDecimal.Field<usrAdjustedTotalQty> { }

        [PXQuantity]
        [PXUIField(DisplayName = "Adjusted Total Qty.", Enabled = false)]
        public virtual Decimal? UsrAdjustedTotalQty { get; set; }
        #endregion

        #region UsrCurrentTotalTranCost
        public abstract class usrCurrentTotalTranCost : PX.Data.BQL.BqlDecimal.Field<usrCurrentTotalTranCost> { }

        [PXPriceCost]
        [PXUIField(DisplayName = "Current Total Cost", Enabled = false)]
        public virtual Decimal? UsrCurrentTotalTranCost { get; set; }
        #endregion

        #region UsrAdjustedTotalTranCost
        public abstract class usrAdjustedTotalTranCost : PX.Data.BQL.BqlDecimal.Field<usrAdjustedTotalTranCost> { }

        [PXPriceCost]
        [PXUIField(DisplayName = "Adjusted Total Cost", Enabled = false)]
        public virtual Decimal? UsrAdjustedTotalTranCost { get; set; }
        #endregion
    }
}