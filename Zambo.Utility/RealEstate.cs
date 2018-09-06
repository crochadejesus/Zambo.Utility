namespace WSAsset.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RealEstate")]
    public partial class RealEstate
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RealEstate()
        {
            RealEstateAttributeValue = new HashSet<RealEstateAttributeValue>();
        }

        public int RealEstateID { get; set; }

        public int AssetID { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public int RealEstateStatusID { get; set; }

        public int RealEstateTypeID { get; set; }

        public int? RealEstateTipologyID { get; set; }

        public int RealEstateConditionID { get; set; }

        public int CountryID { get; set; }

        public int? DistrictID { get; set; }

        public int? CountyID { get; set; }

        public int? ParishID { get; set; }

        [StringLength(500)]
        public string Address { get; set; }

        public decimal? GrossBuildingArea { get; set; }

        public int? ConstructionYear { get; set; }

        public bool? HasParking { get; set; }

        public int? EnergyCertificateID { get; set; }

        [StringLength(50)]
        public string EnergyCertificateNumber { get; set; }

        [StringLength(8)]
        public string ZipCode { get; set; }

        public virtual Asset Asset { get; set; }

        public virtual Country Country { get; set; }

        public virtual County County { get; set; }

        public virtual District District { get; set; }

        public virtual Parish Parish { get; set; }

        public virtual RealEstateEnergyCertificate RealEstateEnergyCertificate { get; set; }

        public virtual RealEstateCondition RealEstateCondition { get; set; }

        public virtual RealEstateStatus RealEstateStatus { get; set; }

        public virtual RealEstateTipology RealEstateTipology { get; set; }

        public virtual RealEstateType RealEstateType { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RealEstateAttributeValue> RealEstateAttributeValue { get; set; }
    }
}
namespace WSAsset.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RealEstateAttribute")]
    public partial class RealEstateAttribute
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RealEstateAttribute()
        {
            RealEstateAttributeLocalized = new HashSet<RealEstateAttributeLocalized>();
            RealEstateAttributeValue = new HashSet<RealEstateAttributeValue>();
        }

        public int RealEstateAttributeID { get; set; }

        public int DataTypeID { get; set; }

        public int RealEstateAttributeCategoryID { get; set; }

        [StringLength(20)]
        public string Unit { get; set; }

        [Required]
        [StringLength(100)]
        public string Description { get; set; }

        public int Order { get; set; }

        public bool IsActive { get; set; }

        public virtual DataType DataType { get; set; }

        public virtual RealEstateAttributeCategory RealEstateAttributeCategory { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RealEstateAttributeLocalized> RealEstateAttributeLocalized { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RealEstateAttributeValue> RealEstateAttributeValue { get; set; }
    }
}
namespace WSAsset.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RealEstateAttributeCategory")]
    public partial class RealEstateAttributeCategory
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RealEstateAttributeCategory()
        {
            RealEstateAttribute = new HashSet<RealEstateAttribute>();
            RealEstateAttributeCategoryLocalized = new HashSet<RealEstateAttributeCategoryLocalized>();
        }

        public int RealEstateAttributeCategoryID { get; set; }

        [Required]
        [StringLength(100)]
        public string Description { get; set; }

        public int Order { get; set; }

        public bool IsActive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RealEstateAttribute> RealEstateAttribute { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RealEstateAttributeCategoryLocalized> RealEstateAttributeCategoryLocalized { get; set; }
    }
}
namespace WSAsset.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RealEstateAttributeCategoryLocalized")]
    public partial class RealEstateAttributeCategoryLocalized
    {
        public int RealEstateAttributeCategoryLocalizedID { get; set; }

        public int RealEstateAttributeCategoryID { get; set; }

        public int LanguageID { get; set; }

        [Required]
        [StringLength(100)]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public virtual Language Language { get; set; }

        public virtual RealEstateAttributeCategory RealEstateAttributeCategory { get; set; }
    }
}
namespace WSAsset.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RealEstateAttributeLocalized")]
    public partial class RealEstateAttributeLocalized
    {
        public int RealEstateAttributeLocalizedID { get; set; }

        public int RealEstateAttributeID { get; set; }

        public int LanguageID { get; set; }

        [Required]
        [StringLength(100)]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public virtual Language Language { get; set; }

        public virtual RealEstateAttribute RealEstateAttribute { get; set; }
    }
}
namespace WSAsset.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RealEstateAttributeValue")]
    public partial class RealEstateAttributeValue
    {
        [Key]
        public int RealEstateAttributeDetailTextID { get; set; }

        public int RealEstateAttributeID { get; set; }

        public int RealEstateID { get; set; }

        public decimal? NumberValue { get; set; }

        public bool? BoolValue { get; set; }

        [StringLength(2000)]
        public string TextValue { get; set; }

        public DateTime? DateValue { get; set; }

        public bool IsActive { get; set; }

        public virtual RealEstate RealEstate { get; set; }

        public virtual RealEstateAttribute RealEstateAttribute { get; set; }
    }
}
namespace WSAsset.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RealEstateCondition")]
    public partial class RealEstateCondition
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RealEstateCondition()
        {
            RealEstate = new HashSet<RealEstate>();
            RealEstateConditionLocalized = new HashSet<RealEstateConditionLocalized>();
        }

        public int RealEstateConditionID { get; set; }

        [Required]
        [StringLength(50)]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RealEstate> RealEstate { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RealEstateConditionLocalized> RealEstateConditionLocalized { get; set; }
    }
}
namespace WSAsset.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RealEstateConditionLocalized")]
    public partial class RealEstateConditionLocalized
    {
        public int RealEstateConditionLocalizedID { get; set; }

        public int RealEstateConditionID { get; set; }

        public int LanguageID { get; set; }

        [Required]
        [StringLength(100)]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public virtual Language Language { get; set; }

        public virtual RealEstateCondition RealEstateCondition { get; set; }
    }
}
namespace WSAsset.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RealEstateEnergyCertificate")]
    public partial class RealEstateEnergyCertificate
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RealEstateEnergyCertificate()
        {
            RealEstate = new HashSet<RealEstate>();
        }

        public int RealEstateEnergyCertificateID { get; set; }

        [Required]
        [StringLength(50)]
        public string Description { get; set; }

        [StringLength(10)]
        public string IsActive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RealEstate> RealEstate { get; set; }
    }
}
namespace WSAsset.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RealEstateEnergyCertificateLocalized")]
    public partial class RealEstateEnergyCertificateLocalized
    {
        public int RealEstateEnergyCertificateLocalizedID { get; set; }

        public int RealEstateEnergyCertificateID { get; set; }

        public int LanguageID { get; set; }

        [Required]
        [StringLength(20)]
        public string Description { get; set; }

        public bool IsActive { get; set; }
    }
}
namespace WSAsset.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class RealEstateStatus
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RealEstateStatus()
        {
            RealEstate = new HashSet<RealEstate>();
            RealEstateStatusLocalized = new HashSet<RealEstateStatusLocalized>();
        }

        public int RealEstateStatusID { get; set; }

        [Required]
        [StringLength(50)]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RealEstate> RealEstate { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RealEstateStatusLocalized> RealEstateStatusLocalized { get; set; }
    }
}
namespace WSAsset.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RealEstateStatusLocalized")]
    public partial class RealEstateStatusLocalized
    {
        public int RealEstateStatusLocalizedID { get; set; }

        public int RealEstateStatusID { get; set; }

        public int LanguageID { get; set; }

        [Required]
        [StringLength(100)]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public virtual Language Language { get; set; }

        public virtual RealEstateStatus RealEstateStatus { get; set; }
    }
}
namespace WSAsset.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RealEstateTipology")]
    public partial class RealEstateTipology
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RealEstateTipology()
        {
            RealEstate = new HashSet<RealEstate>();
            RealEstateTipologyLocalized = new HashSet<RealEstateTipologyLocalized>();
        }

        public int RealEstateTipologyID { get; set; }

        [Required]
        [StringLength(50)]
        public string Description { get; set; }

        public int Order { get; set; }

        public bool IsActive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RealEstate> RealEstate { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RealEstateTipologyLocalized> RealEstateTipologyLocalized { get; set; }
    }
}
namespace WSAsset.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RealEstateTipologyLocalized")]
    public partial class RealEstateTipologyLocalized
    {
        public int RealEstateTipologyLocalizedID { get; set; }

        public int RealEstateTipologyID { get; set; }

        public int LanguageID { get; set; }

        [Required]
        [StringLength(100)]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public virtual Language Language { get; set; }

        public virtual RealEstateTipology RealEstateTipology { get; set; }
    }
}
namespace WSAsset.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RealEstateType")]
    public partial class RealEstateType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RealEstateType()
        {
            RealEstate = new HashSet<RealEstate>();
            RealEstateTypeLocalized = new HashSet<RealEstateTypeLocalized>();
        }

        public int RealEstateTypeID { get; set; }

        [Required]
        [StringLength(100)]
        public string Description { get; set; }

        public bool? IsActive { get; set; }

        public bool ShowInHomePage { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RealEstate> RealEstate { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RealEstateTypeLocalized> RealEstateTypeLocalized { get; set; }
    }
}
namespace WSAsset.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RealEstateTypeLocalized")]
    public partial class RealEstateTypeLocalized
    {
        public int RealEstateTypeLocalizedID { get; set; }

        public int RealEstateTypeID { get; set; }

        public int LanguageID { get; set; }

        [Required]
        [StringLength(100)]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public virtual Language Language { get; set; }

        public virtual RealEstateType RealEstateType { get; set; }
    }
}
