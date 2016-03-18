using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenMap
{
    /// <summary>
	/// Converts from/to Location structure.
	/// </summary>
	public class LocationConverter : TypeConverter
    {
        /// <summary>
        /// Gets a value that indicates whether this type converter can convert from a given type. 
        /// </summary>
        /// <param name="context">ITypeDescriptorContext for this call.</param>
        /// <param name="sourceType">Type being queried for support.</param>
        /// <returns>True if this converter can convert from the specified type; false otherwise.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// Attempts to convert to a Location from the given object. 
        /// </summary>
        /// <param name="context">The ITypeDescriptorContext for this call.</param>
        /// <param name="culture">The CultureInfo which is respected when converting.</param>
        /// <param name="value">The object to convert to an instance of Location. </param>
        /// <returns>Location that was constructed.</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string str = value as string;
            if (!string.IsNullOrEmpty(str))
            {
                Location coordinates = Location.Parse(str);
                return coordinates;
            }

            return null;
        }
    }
}
