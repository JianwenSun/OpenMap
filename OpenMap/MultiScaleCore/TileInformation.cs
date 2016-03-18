using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenMap
{
    /// <summary>
    /// Represents information about tile.
    /// </summary>
    internal struct TileInformation
    {
        /// <summary>
        /// Gets or sets X position of the tile.
        /// </summary>
        public int X
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Y position of the tile.
        /// </summary>
        public int Y
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets layer of the tile.
        /// </summary>
        public int Layer
        {
            get;
            set;
        }
    }
}
