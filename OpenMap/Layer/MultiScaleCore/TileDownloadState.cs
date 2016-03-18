using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenMap
{
    /// <summary>
	/// Represents the TileDownloadState class.
	/// </summary>
	internal class TileDownloadState
    {
        public TileId TileId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets tile image.
        /// </summary>
        internal byte[] TileBody
        {
            get;
            set;
        }

        internal Uri Uri
        {
            get;
            set;
        }

        internal Action<TileDownloadState> Callback
        {
            get;
            set;
        }

        public void Dispose()
        {
            this.TileBody = null;
        }

        /// <summary>
        /// Raises this event when the cache complete the Request of tile.
        /// </summary>
        /// <param name="tile">Returned tile.</param>
        internal void RaiseEvent(byte[] tile)
        {
            this.TileBody = tile;
            this.Callback(this);
        }
    }
}
