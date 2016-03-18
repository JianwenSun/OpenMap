using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenMap
{
    public class MouseDragStartedRoutedEventArgs : RoutedEventArgs
    {
        public Point Origin { get; set; }

        public MouseDragStartedRoutedEventArgs(RoutedEvent routeEvent, Point origin)
            : base(routeEvent)
        {
            this.Origin = origin;
        }
    }
}
