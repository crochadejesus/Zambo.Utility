namespace WSAsset.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("NewsletterSubscription")]
    public partial class NewsletterSubscription
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public NewsletterSubscription()
        {
            NewsletterGenerationDetail = new HashSet<NewsletterGenerationDetail>();
        }

        public int NewsletterSubscriptionID { get; set; }

        public Guid UniqueID { get; set; }

        public DateTime SubscriptionDate { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        public int LanguageID { get; set; }

        public int CountryID { get; set; }

        public int? DistrictID { get; set; }

        public int? CountyID { get; set; }

        public bool IsCanceled { get; set; }

        public DateTime? CancelDate { get; set; }

        public bool IsActive { get; set; }

        public virtual Country Country { get; set; }

        public virtual County County { get; set; }

        public virtual District District { get; set; }

        public virtual Language Language { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NewsletterGenerationDetail> NewsletterGenerationDetail { get; set; }
    }
}
