using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenMap
{
    public class SpatialReferenceEventArgs : System.EventArgs
    {
        public ISpatialReference OldValue { get; set; }
        public ISpatialReference NewValue { get; set; }

        public SpatialReferenceEventArgs(ISpatialReference oldValue, ISpatialReference newValue)
        {
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }
    }
}
