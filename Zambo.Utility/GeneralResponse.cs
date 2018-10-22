
namespace Value.Domain.Models.Common
{
    public struct GeneralResponse<T>
    {
        public string Message { get; set; }
        public T Response { get; set; }
        public bool Success { get; set; }
    }
}
