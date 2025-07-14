using System;
using Microsoft.EntityFrameworkCore;

namespace API.Helpers;

public class PaginatedResult<T>
{
    public PaginationMetadata Metadata { get; set; } = default!;
    public List<T> Items { get; set; } = [];
};

public class PaginationMetadata
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
};

public class PaginationHelper
{
    public static async Task<PaginatedResult<T>> CreateAsync<T>(IQueryable<T> query,
        int pageNumber, int pageSize)
    {
        var count = await query.CountAsync(); // count how many items in table
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(); // execute request to db

        return new PaginatedResult<T>
        {
            Metadata = new PaginationMetadata
            {
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(count / (double)pageSize), // 25 items | 3 pages | 10 items per page
                PageSize = pageSize,
                TotalCount = count
            },
            Items = items
        };
    }
}

