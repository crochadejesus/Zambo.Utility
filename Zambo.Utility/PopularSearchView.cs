namespace WSAsset.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PopularSearchView")]
    public partial class PopularSearchView
    {
        [Key]
        public int SearchID { get; set; }

        public int LanguageID { get; set; }

        public int SearchRank { get; set; }

        public int DistrictID { get; set; }

        public string District { get; set; }

        public int CountyID { get; set; }

        public string County { get; set; }

        public int RealEstateTypeID { get; set; }

        public string RealEstateType { get; set; }
    }
}
