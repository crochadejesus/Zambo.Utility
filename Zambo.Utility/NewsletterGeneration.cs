namespace WSAsset.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("NewsletterGenerationDetail")]
    public partial class NewsletterGenerationDetail
    {
        public int NewsletterGenerationDetailID { get; set; }

        public int NewsletterGenerationID { get; set; }

        public int NewsletterSubscriptionID { get; set; }

        public Guid UniqueID { get; set; }

        public DateTime? ConsultedDate { get; set; }

        public bool IsActive { get; set; }

        public virtual NewsletterGeneration NewsletterGeneration { get; set; }

        public virtual NewsletterSubscription NewsletterSubscription { get; set; }
    }
}
