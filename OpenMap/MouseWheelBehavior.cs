using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenMap
{
    /// <summary>
	/// This enumeration specifies behavior of mouse wheel.
	/// </summary>
	public enum MouseWheelBehavior
    {
        /// <summary>
        /// The empty action.
        /// </summary>
        None,

        /// <summary>
        /// Mouse wheel zooms in to the point on the map.
        /// </summary>
        ZoomToPoint,

        /// <summary>
        /// Mouse wheel zooms in the map.
        /// </summary>
        ZoomToCenter
    }
}
