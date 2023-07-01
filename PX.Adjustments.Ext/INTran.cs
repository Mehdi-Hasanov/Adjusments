using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.Common.Attributes;
using PX.Objects.Common.Bql;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.GL;
using PX.Objects.IN.Attributes;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.PO;
using PX.Objects.SO;
using PX.Objects;
using System.Collections.Generic;
using System;

namespace PX.Objects.IN
{
    public class INTranExt : PXCacheExtension<PX.Objects.IN.INTran>
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

        [PXDBDecimal]
        [PXUIField(DisplayName = "Current Ext. Cost")]
        public virtual Decimal? UsrCurrentTranCost { get; set; }
        #endregion

        #region UsrAdjustedTranCost
        public abstract class usrAdjustedTranCost : PX.Data.BQL.BqlDecimal.Field<usrAdjustedTranCost> { }

        [PXDecimal]
        [PXUIField(DisplayName = "Adjusted Ext. Cost")]
        public virtual Decimal? UsrAdjustedTranCost { get; set; }
        #endregion
    }
}