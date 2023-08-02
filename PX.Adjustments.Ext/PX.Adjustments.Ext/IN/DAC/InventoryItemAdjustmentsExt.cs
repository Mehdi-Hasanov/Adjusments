using PX.Data;
using PX.Objects.CS;
using PX.Objects.GDPR;
using PX.Objects.IN;
using System;

namespace PX.Adjustments.Ext
{
    public sealed class InventoryItemAdjustmentsExt : PXCacheExtension<InventoryItem>
    {
        public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.inventory>();

        #region LotSerTrack
        public abstract class lotSerTrack : PX.Data.BQL.BqlDecimal.Field<lotSerTrack> { }

        [PXString(1)]
        [PXUnboundDefault(typeof(Search<INLotSerClass.lotSerTrack,
        Where<INLotSerClass.lotSerClassID,
            Equal<Current<InventoryItem.lotSerClassID>>>>))]
        public string LotSerTrack { get; set; }
        #endregion
    }
}