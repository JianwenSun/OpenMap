using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenMap
{
    /// <summary>
	/// Event data of the MapSourceChanged event.
	/// </summary>
	public class MapSourceChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets unique ID of the new map source.
        /// </summary>
        public string NewSourceId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets unique ID of the old map source.
        /// </summary>
        public string OldSourceId
        {
            get;
            set;
        }
    }
}
