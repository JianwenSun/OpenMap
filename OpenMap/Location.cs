using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenMap
{
    [TypeConverterAttribute(typeof(LocationConverter))]
    public struct Location
    {
        /// <summary>
		/// Empty Location.
		/// </summary>
		private static Location empty = CreateEmpty();

        /// <summary>
        /// latitude-coordinate of map. 
        /// </summary>
        private double latitude;

        /// <summary>
        /// longitude-coordinate of map. 
        /// </summary>
        private double longitude;

        /// <summary>
        /// Initializes a new instance of the Location structure.
        /// </summary>
        /// <param name="latitude">Latitude.</param>
        /// <param name="longitude">Longitude.</param>
        public Location(double latitude, double longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
        }

        /// <summary>
        /// Gets a value that represents an empty Location structure.
        /// </summary>
        /// <remarks>latitude,longitude both were negative infinity.
        /// </remarks>
        public static Location Empty
        {
            get
            {
                return Location.empty;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether this Location structure is empty.
        /// </summary>
        /// <remarks>A Location structure with Latitude and Longitude values set to 0 is not empty. An empty Location 
        /// structure has Latitude and Longitude values set to negative infinity. This is the only time a Location 
        /// structure can have negative infinity values.</remarks>
        public bool IsEmpty
        {
            get
            {
                return double.IsNegativeInfinity(this.latitude)
                    && double.IsNegativeInfinity(this.longitude);
            }
        }

        /// <summary>
        /// Get or sets x.
        /// </summary>
        public double Latitude
        {
            get { return this.latitude; }
            set
            {
                this.latitude = value;
            }
        }

        /// <summary>
        /// Get or sets y.
        /// </summary>
        public double Longitude
        {
            get { return this.longitude; }
            set
            {
                this.longitude = value;
            }
        }

        /// <summary>
        /// Converts a String representation of a Latitude-Longitude into the equivalent Location object.
        /// </summary>
        /// <param name="source">The String representation of the Location object.</param>
        /// <returns>The equivalent Location structure.</returns>
        public static Location Parse(string source)
        {
            if (string.IsNullOrEmpty(source))
                throw new ArgumentNullException("source");

            try
            {
                string[] pair = source.Split(',');
                if (pair.Length != 2)
                    throw new FormatException("Input string was not in a correct format.");
                return new Location(double.Parse(pair[0], CultureInfo.InvariantCulture), double.Parse(pair[1], CultureInfo.InvariantCulture));
            }
            catch (Exception exc)
            {
                throw new FormatException("Input string was not in a correct format.", exc);
            }
        }

        /// <summary>
        /// Compares two Location structures. 
        /// </summary>
        /// <param name="location1">The instance of Location to compare. </param>
        /// <param name="location2">The instance of Location to compare. </param>
        /// <returns>true if instances are equal; otherwise, false.</returns>
        public static bool Equals(Location location1, Location location2)
        {
            return location1.Latitude == location2.Latitude && location1.Longitude == location2.Longitude;
        }

        /// <summary>
        /// Converts this Location structure into a Point structure.
        /// </summary>
        /// <param name="location">The Location to convert.</param>
        /// <returns>The result of converting.</returns>
        public static explicit operator Point(Location location)
        {
            return new Point(location.Latitude, location.Longitude);
        }

        /// <summary>
        /// Compares two Location structures for equality. 
        /// </summary>
        /// <param name="location1">The instance of Location to compare. </param>
        /// <param name="location2">The instance of Location to compare. </param>
        /// <returns>true if instances are equal; otherwise, false.</returns>
        public static bool operator ==(Location location1, Location location2)
        {
            return location1.Equals(location2);
        }

        /// <summary>
        /// Compares two Location structures for inequality. 
        /// </summary>
        /// <param name="location1">The instance of Location to compare. </param>
        /// <param name="location2">The instance of Location to compare. </param>
        /// <returns>true if instances are equal; otherwise, false.</returns>
        public static bool operator !=(Location location1, Location location2)
        {
            return !location1.Equals(location2);
        }

        /// <summary>
        /// Compares two Location structures for equality. 
        /// </summary>
        /// <param name="obj">The instance of Location to compare to this instance. </param>
        /// <returns>true if instances are equal; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Location))
                return false;
            return Equals(this, (Location)obj);
        }

        /// <summary>
        /// Gets a hash code for this Location structure.
        /// </summary>
        /// <returns>A hash code for this Location structure.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Creates a String representation of this Location object using KML format.
        /// KML uses sequence of the Latitude and Longitude. 
        /// Longitude is first and the Latitude is second.
        /// </summary>
        /// <param name="provider">The culture-specific formatting information.</param>
        /// <returns>A String containing the Latitude and Longitude values of this Location object in KML format.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Kml")]
        public string ToKmlString(IFormatProvider provider)
        {
            NumberFormatInfo formatInfo = provider.GetFormat(typeof(NumberFormatInfo)) as NumberFormatInfo;
            string listSeparator = formatInfo == null || formatInfo.NumberDecimalSeparator != "," ? "," : ";";
            return this.Latitude.ToString(provider) + listSeparator + this.Longitude.ToString(provider);
        }

        /// <summary>
        /// Creates a String representation of this Location object.
        /// </summary>
        /// <returns>A String containing the Latitude and Longitude values of this Location object.</returns>
        public override string ToString()
        {
            return this.ToString(CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Creates a String representation of this Location object. 
        /// </summary>
        /// <param name="provider">The culture-specific formatting information. </param>
        /// <returns>A String containing the Latitude and Longitude values of this Location object.</returns>
        public string ToString(IFormatProvider provider)
        {
            NumberFormatInfo formatInfo = provider.GetFormat(typeof(NumberFormatInfo)) as NumberFormatInfo;
            string listSeparator = formatInfo == null || formatInfo.NumberDecimalSeparator != "," ? "," : ";";
            return this.Latitude.ToString(provider) + listSeparator + this.Longitude.ToString(provider);
        }

        /// <summary>
        /// Creates empty Location structure.
        /// </summary>
        /// <returns></returns>
        private static Location CreateEmpty()
        {
            return new Location() { latitude = double.NegativeInfinity, longitude = double.NegativeInfinity };
        }
    }
}
