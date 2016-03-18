using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenMap
{
    /// <summary>
	/// Mercator is the projection used by most online maps including Virtual Earth, GMaps and Yahoo
	/// It maintains Vertical resolution while expanding the earth horizontally to fill
	/// The effect is that items at the poles appear much larger then those of equal size at the equator.
	/// </summary>
	public class MercatorProjection : SpatialReference
    {
        /// <summary>
        /// Initializes a new instance of the MercatorProjection class.
        /// Mercator is the projection used by most online maps including Virtual Earth, GMaps and Yahoo.
        /// </summary>
        public MercatorProjection()
        {
            this.GeoGcs = "GCS_WGS_1984";
            this.SpheroidRadius = 6378137.00D;
            this.UnitAuthority = "Meter";

            this.ScaleX = SpatialReference.HalfPI;
            this.ScaleY = -SpatialReference.HalfPI;
            this.OffsetX = 0.5;
            this.OffsetY = 0.5;

            this.MinLatitude = -85.05112878D;
            this.MaxLatitude = 85.05112878D;
            this.MinLongitude = -180D;
            this.MaxLongitude = 180D;
        }
    }
}
