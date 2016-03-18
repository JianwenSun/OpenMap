using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenMap
{
    public class MouseDragedRoutedEventArgs : RoutedEventArgs
    {
        public Point Origin { get; set; }

        public Point Target { get; set; }

        public MouseDragedRoutedEventArgs(RoutedEvent routedEvent, Point origin, Point traget)
            :base(routedEvent)
        {

            this.Origin = origin;
            this.Target = traget;
        }
    }
}
