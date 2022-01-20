using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ObermindPurchaseOrder.Api.Models;

namespace ObermindPurchaseOrder.Api.Helpers
{
    public static class PaginationListGenerator
    {
        public static async Task<PaginatedList<T>> Paginate<T>(this IQueryable<T> query, int pageIndex, int pageSize) where T : class
        {
            var data = new PaginatedList<T>
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalRecords = (int) await query.LongCountAsync()
            };

            var pageCount = (double) data.TotalRecords / pageSize;
            data.TotalPages = (int)Math.Ceiling(pageCount);
            data.Data = query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            return data;
        }
    }
}