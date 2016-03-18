using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenMap
{
    /// <summary>
	/// The OGC Spatial Reference requirements.
	/// </summary>
	public class SpatialReference : ISpatialReference
    {
        /// <summary>
        /// Half of PI.
        /// </summary>
        public const double HalfPI = 0.159154943091895;

        /// <summary>
        /// Degrees of one radiant.
        /// </summary>
        public const double RadiansToDegrees = 57.2957795130823;

        /// <summary>
		/// A coordinate system based on latitude and longitude. Some geographic coordinate systems are Latitude/Longitude, 
		/// and some are Longitude/Latitude. You can find out which this is by examining the axes. 
		/// You should also check the angular units, since not all geographic coordinate systems use degrees.
		/// </summary>
        public string GeoGcs
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the authority body that defines the standards for the spatial reference parameters.
        /// The Spatial Reference is WGS 1984 and the authority is EPSG:4326.
        /// </summary>
        public string Authority
        {
            get;
            set;
        }

        /// <summary>
        /// The authority body that defines the unit of measurement i.e. European Petroleum Survey Group (EPSG).
        /// The unit of measurement is usually degrees and the authority for the datum the map uses, WGS 1984 is EPSG:4326.
        /// </summary>
        public string UnitAuthority
        {
            get;
            set;
        }
        
        /// <summary>
         /// Gets or sets a spheroid, which is an approximation of the Earth's surface as a squashed sphere.
         /// </summary>
        public double SpheroidRadius
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the real world coordinate scale at a given longitude.
        /// </summary>
        public double ScaleX
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the real world coordinate scale at a given latitude.
        /// </summary>
        public double ScaleY
        {
            get;
            set;
        }

        /// <summary>
		/// Gets or sets logical X offset to centre of earth.
		/// </summary>
		public double OffsetX
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets logical Y offset to centre of earth.
        /// </summary>
        public double OffsetY
        {
            get;
            set;
        }

        /// <summary>
        /// The minimum Latitude this tile source supports.
        /// </summary>
        public double MinLatitude
        {
            get;
            set;
        }

        /// <summary>
        /// The maximum Latitude this tile source supports.
        /// </summary>
        public double MaxLatitude
        {
            get;
            set;
        }

        /// <summary>
        /// The minimum Longitude this tile source supports.
        /// </summary>
        public double MinLongitude
        {
            get;
            set;
        }

        /// <summary>
        /// The maximum Longitude this tile source supports.
        /// </summary>
        public double MaxLongitude
        {
            get;
            set;
        }

        /// <summary>
		/// Converts a geographical coordinate (Longitude, Latitude) to a logical Point (0->1).
		/// </summary>
		/// <param name="geographicPoint">The geographical coordinate (Longitude, Latitude).</param>
		/// <returns>The logical Point.</returns>
		public virtual Point GeographicToLogical(Location geographicPoint)
        {
            double d = Math.Sin(geographicPoint.Latitude * (Math.PI * 2 / 360));

            Point point = new Point((geographicPoint.Longitude * (Math.PI * 2 / 360) * this.ScaleX) + this.OffsetX,
                (0.5 * Math.Log((1.0 + d) / (1.0 - d)) * this.ScaleY) + this.OffsetY);

            if (point.Y > 1)
            {
                point.Y = 1;
            }

            return point;
        }

        /// <summary>
        /// Converts a logical Point (0->1) to a geographical coordinate (Longitude, Latitude).
        /// </summary>
        /// <param name="logicalPoint">The logical Point.</param>
        /// <returns>The geographical coordinate (Longitude, Latitude).</returns>
        public virtual Location LogicalToGeographic(Point logicalPoint)
        {
            return new Location(
                Math.Atan(Math.Sinh((logicalPoint.Y - this.OffsetY) / this.ScaleY)) * RadiansToDegrees,
                (logicalPoint.X - this.OffsetX) * RadiansToDegrees / this.ScaleX);
        }
    }
}
