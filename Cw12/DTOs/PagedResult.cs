using System.Collections.Generic;

namespace Cw12.DTOs
{
    public class PagedResult<T>
    {
        public int PageNum { get; set; }
        public int PageSize { get; set; }
        public int AllPages { get; set; }
        public IEnumerable<T> Items { get; set; }
    }
}