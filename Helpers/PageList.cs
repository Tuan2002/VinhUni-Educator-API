
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace VinhUni_Educator_API.Helpers
{
    public class PageList<T>
    {
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public List<T> Items { get; set; } = new List<T>();
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
        public PageList(List<T> items, int count, int pageIndex, int pageSize)
        {
            TotalCount = count;
            PageIndex = pageIndex;
            PageSize = pageSize;
            Items.AddRange(items);
        }

        public static async Task<PageList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.OrderByDescending(x => x).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PageList<T>(items, count, pageIndex, pageSize);
        }
    }
}