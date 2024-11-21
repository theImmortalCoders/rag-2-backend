#region

using System.Linq.Expressions;
using rag_2_backend.Infrastructure.Common.Model;

#endregion

namespace rag_2_backend.Infrastructure.Util;

public static class DataSortingUtil
{
    public static IQueryable<T> ApplySorting<T, TKey>(IQueryable<T> query, Expression<Func<T, TKey>> keySelector,
        SortDirection sortDirection)
    {
        return sortDirection == SortDirection.Desc ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }
}