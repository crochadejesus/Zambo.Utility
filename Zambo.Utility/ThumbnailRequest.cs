
namespace Value.Domain.Models.Valuations.Request
{
    public class ThumbnailsRequest
    {
        public short Height { get; set; }
        public long? DocumentID { get; set; }
        public short Width { get; set; }
        public long RequestID { get; set; }
    }
}
