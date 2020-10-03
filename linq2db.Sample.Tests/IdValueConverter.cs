using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace linq2db.Sample.Tests
{
    [PublicAPI]
    public sealed class IdValueConverter<TId, T> : ValueConverter<Id<T, TId>, TId>
        where T : IHasId<T, TId>
    {
        public IdValueConverter(ConverterMappingHints mappingHints = null)
            : base(id => id, id =>  id.AsId<T, TId>()) { }
    }
}