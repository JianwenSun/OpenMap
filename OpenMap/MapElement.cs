using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenMap
{
    public class MapElement : UIElement, IMapElement
    {
        public Location Location
        {
            get;
            set;
        }
    }
}
