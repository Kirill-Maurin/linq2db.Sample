using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Diagnostics.CodeAnalysis;

namespace linq2db.Sample.Tests
{
    public sealed class IdValueConverterSelector<T> : ValueConverterSelector
        where T: IValueConverterSelector
    {
        public IdValueConverterSelector([NotNull] ValueConverterSelectorDependencies dependencies) : base(dependencies) 
            => _selector = (IValueConverterSelector)Activator.CreateInstance(typeof(T), dependencies);

        public override IEnumerable<ValueConverterInfo> Select(Type modelClrType, Type providerClrType = null)
        {
            return TryGetIdConverter(out var converter) ? new [] {converter} : _selector.Select(modelClrType, providerClrType); 

            bool TryGetIdConverter(out ValueConverterInfo converter)
            {
                converter = default;
                modelClrType = modelClrType.UnwrapNullable();
                providerClrType = providerClrType.UnwrapNullable();

                if (!modelClrType.IsGenericType)
                    return false;

                if (modelClrType.GetGenericTypeDefinition() != typeof(Id<,>))
                    return false;

                var t = modelClrType.GetGenericArguments();
                var key = t[1];
                providerClrType ??= key;
                if (key != providerClrType)
                    return false;

                var ct = typeof(IdValueConverter<,>).MakeGenericType(key, t[0]);
                converter =  new ValueConverterInfo
                (
                    modelClrType,
                    providerClrType,
                    i => (ValueConverter) Activator.CreateInstance(ct, i.MappingHints)
                );
                return true;
            }
        }

        readonly IValueConverterSelector _selector;
    }
}