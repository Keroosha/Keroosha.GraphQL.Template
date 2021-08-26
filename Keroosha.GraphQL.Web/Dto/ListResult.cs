using System;
using System.Collections.Generic;
using System.Linq;

namespace Keroosha.GraphQL.Web.Dto
{
    public class ListResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }

        public ListResult()
        {
        }

        public ListResult(List<T> items, int totalCount)
        {
            Items = items;
            TotalCount = totalCount;
        }

        public ListResult<TOther> Map<TOther>(Func<T, TOther> selector)
            => new ListResult<TOther>(Items.Select(selector).ToList(), TotalCount);
    }

    public static class ListResultExtensions
    {
        public static ListResult<T> AsListResult<T>(this IQueryable<T> q, int skip, int take)
            => new ListResult<T>(q.Skip(skip).Take(take).ToList(), q.Count());
    }
}