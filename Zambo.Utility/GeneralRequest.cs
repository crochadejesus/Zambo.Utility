namespace Value.Domain.Models.Common.Request
{
    public struct GeneralRequest<T>
    {
        public T Request { get; set; }
        public string Token { get; set; }
    }
}
