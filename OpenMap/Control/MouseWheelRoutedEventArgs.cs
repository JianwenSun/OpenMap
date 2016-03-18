using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenMap
{
    public class MouseWheelRoutedEventArgs : RoutedEventArgs
    {
        public Point Origin { get; set; }
        public double Delta { get; set; }

        public MouseWheelRoutedEventArgs(RoutedEvent routeEvent, Point origin, double delta)
            : base(routeEvent)
        {
            this.Origin = origin;
            this.Delta = delta;
        }
    }
}
