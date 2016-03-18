using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenMap
{
    /// <summary>
    /// Represents map layers. 
    /// </summary>
    public enum TOpenStreetMapLayer
    {
        /// <summary>
        /// Standard layer.
        /// </summary>
        Standard,

        /// <summary>
        /// Cycle layer.
        /// </summary>
        Cycle,

        /// <summary>
        /// Transport layer.
        /// </summary>
        Transport,

        /// <summary>
        /// MapQuest layer.
        /// </summary>
        MapQuest,

        /// <summary>
        /// Humanitarian layer.
        /// </summary>
        Humanitarian
    }
}
