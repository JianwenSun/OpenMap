using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenMap
{
    /// <summary>
	/// Represents map animation modes.
	/// </summary>
	public enum SpringAnimationsMode
    {
        /// <summary>
        /// All mode. Enables zooming and panning animation.
        /// </summary>
        All,

        /// <summary>
        /// Panning mode. Enables panning animation only.
        /// </summary>
        Panning,

        /// <summary>
        /// Zooming mode. Enables zooming animation only.
        /// </summary>
        Zooming
    }
}
