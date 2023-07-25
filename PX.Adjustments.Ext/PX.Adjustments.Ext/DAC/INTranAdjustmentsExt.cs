using PX.Data;
using System;

namespace PX.Objects.IN
{
    public class INTranAdjustmentsExt : PXCacheExtension<PX.Objects.IN.INTran>
    {
        #region UsrCurrentQtyOnHand
        public abstract class usrCurrentQtyOnHand : PX.Data.BQL.BqlDecimal.Field<usrCurrentQtyOnHand> { }

        [PXDBQuantity]
        [PXUIField(DisplayName = "Current Qty. On Hand")]
        public virtual Decimal? UsrCurrentQtyOnHand { get; set; }
        #endregion

        #region UsrAdjustedQtyOnHand
        public abstract class usrAdjustedQtyOnHand : PX.Data.BQL.BqlDecimal.Field<usrAdjustedQtyOnHand> { }

        [PXQuantity]
        [PXUIField(DisplayName = "Adjusted Qty. On Hand")]
        public virtual Decimal? UsrAdjustedQtyOnHand { get; set; }
        #endregion

        #region UsrCurrentTranCost
        public abstract class usrCurrentTranCost : PX.Data.BQL.BqlDecimal.Field<usrCurrentTranCost> { }

        [PXDBPriceCost]
        [PXUIField(DisplayName = "Current Ext. Cost")]
        public virtual Decimal? UsrCurrentTranCost { get; set; }
        #endregion

        #region UsrAdjustedTranCost
        public abstract class usrAdjustedTranCost : PX.Data.BQL.BqlDecimal.Field<usrAdjustedTranCost> { }

        [PXPriceCost]
        [PXUIField(DisplayName = "Adjusted Ext. Cost")]
        public virtual Decimal? UsrAdjustedTranCost { get; set; }
        #endregion
    }
}