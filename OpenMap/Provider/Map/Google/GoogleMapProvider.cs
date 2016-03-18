using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenMap
{
    public class GoogleMapProvider : TiledProvider
    {
        private MercatorProjection projection = new MercatorProjection();

        /// <summary>
        /// Initializes a new instance of the GoogleMapProvider class.
        /// </summary>
        public GoogleMapProvider()
            : base()
        {
            TiledMapSource source = new GoogleMapSource();
            this.MapSources.Add(source.UniqueId, source);
            this.SetMapSource(source.UniqueId);
        }

        /// <summary>
        /// Returns the SpatialReference for the map provider.
        /// </summary>
        public override ISpatialReference SpatialReference
        {
            get
            {
                return this.projection;
            }
        }
    }
}
