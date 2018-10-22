using System.Collections.Generic;

namespace Value.Domain.Models.Common
{
    public class PagedList<T>
    {
        public IList<T> Rows = null;

        public PagedList()
        {
            Rows = new List<T>();
        }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRows { get; set; }
    }
}
