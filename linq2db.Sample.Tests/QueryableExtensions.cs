using System.Linq;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace linq2db.Sample.Tests
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> AsLinqToDb<T>(this IQueryable<T> queryable, bool l2db)
            => l2db ? queryable.ToLinqToDB() : queryable;

        public static IQueryable<T> AsTracking<T>(this IQueryable<T> queryable, bool tracking) 
            where T : class 
            => tracking ? queryable.AsTracking() : queryable.AsNoTracking();
    }
}