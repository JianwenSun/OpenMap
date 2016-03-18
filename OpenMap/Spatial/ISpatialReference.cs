using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenMap
{
    public interface ISpatialReference
    {
        /// <summary>
		/// A coordinate system based on latitude and longitude. Some geographic coordinate systems are Latitude/Longitude, 
		/// and some are Longitude/Latitude. You can find out which this is by examining the axes. 
		/// You should also check the angular units, since not all geographic coordinate systems use degrees.
		/// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gcs")]
        string GeoGcs
        {
            get;
            set;
        }

        string Authority
        {
            get;
            set;
        }

        /// <summary>
		/// The authority body that defines the unit of measurement i.e. European Petroleum Survey Group (EPSG).
		/// The unit of measurement is usually degrees and the authority for the datum the map uses, WGS 1984 is EPSG:4326.
		/// </summary>
		string UnitAuthority
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a spheroid, which is an approximation of the Earth's surface as a squashed sphere.
        /// </summary>
        double SpheroidRadius
        {
            get;
            set;
        }

        /// <summary>
        /// The minimum Latitude this tile source supports.
        /// </summary>
        double MinLatitude
        {
            get;
            set;
        }

        /// <summary>
        /// The maximum Latitude this tile source supports.
        /// </summary>
        double MaxLatitude
        {
            get;
            set;
        }

        /// <summary>
        /// The minimum Longitude this tile source supports.
        /// </summary>
        double MinLongitude
        {
            get;
            set;
        }

        /// <summary>
        /// The maximum Longitude this tile source supports.
        /// </summary>
        double MaxLongitude
        {
            get;
            set;
        }

        /// <summary>
		/// Converts a geographical coordinate (Latitude, Longitude) to a logical Point (0->1).
		/// </summary>
		/// <param name="geographicPoint">The geographical coordinate (Latitude, Longitude).</param>
		/// <returns>The logical Point.</returns>
        Point GeographicToLogical(Location geographicPoint);

        /// <summary>
		/// Converts a logical Point (0->1) to a geographical coordinate (Latitude, Longitude).
		/// </summary>
		/// <param name="logicalPoint">The logical Point.</param>
		/// <returns>The geographical coordinate (Latitude, Longitude).</returns>
        Location LogicalToGeographic(Point logicalPoint);
    }
}
