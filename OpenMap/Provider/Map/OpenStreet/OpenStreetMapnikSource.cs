using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenMap
{
    /// <summary>
	/// Represents source for the Mapnik renderer.
	/// </summary>
	public class OpenStreetMapnikSource : OpenStreetTiledMapSource
    {
        private const string TileMapnikUrlFormat = @"http://{prefix}.tile.openstreetmap.org/{zoom}/{x}/{y}.png";

        /// <summary>
        /// Initializes a new instance of the OpenStreetMapnikSource class.
        /// </summary>
        public OpenStreetMapnikSource()
            : base(TileMapnikUrlFormat)
        {
        }
    }
}
