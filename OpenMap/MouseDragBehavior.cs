using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenMap
{
    /// <summary>
	/// The possible options when you click and drag your mouse over the map.
	/// </summary>
	public enum MouseDragBehavior
    {
        /// <summary>
        /// The Map will do nothing.
        /// </summary>
        None,

        /// <summary>
        /// The Map will essentially stick to your mouse and move where you drag it to.
        /// </summary>
        Drag,

        /// <summary>
        /// Draw a Marque Selection Box over the map, and perform action on selected rectangle when mouse released.
        /// The action depends on the mouse selection mode.
        /// </summary>
        Select
    }
}
