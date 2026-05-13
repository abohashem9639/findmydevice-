using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FindMyDevice.Infrastructure.Extensions
{
    public static class ValueConversionExtensions
    {
        /// <summary>
        /// Configures a property to store an array of Enums as a PostgreSQL text[] column (string array).
        /// Includes necessary ValueComparer for correct change tracking.
        /// </summary>
        public static PropertyBuilder<TEnum[]> HasEnumArrayConversion<TEnum>(this PropertyBuilder<TEnum[]> propertyBuilder)
            where TEnum : struct, Enum
        {
            var converter = new ValueConverter<TEnum[], string[]>(
                v => v.Select(e => e.ToString()).ToArray(),
                v => v.Select(e => Enum.Parse<TEnum>(e)).ToArray());

            var comparer = new ValueComparer<TEnum[]>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToArray());

            propertyBuilder
                .HasConversion(converter)
                .Metadata.SetValueComparer(comparer);

            propertyBuilder.HasColumnType("text[]");

            return propertyBuilder;
        }
    }
}
