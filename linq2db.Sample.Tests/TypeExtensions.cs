using System;

namespace linq2db.Sample.Tests
{
    public static class TypeExtensions
    {
        public static Type UnwrapNullable(this Type type) 
            => type == null ? null : Nullable.GetUnderlyingType(type) ?? type;
    }
}