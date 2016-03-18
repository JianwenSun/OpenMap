using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenMap
{
    public class MouseClickRoutedEventArgs : RoutedEventArgs
    {
        public Point Origin { get; set; }

        public MouseClickRoutedEventArgs(RoutedEvent routeEvent, Point origin)
            : base(routeEvent)
        {
            this.Origin = origin;
        }
    }

    public class MouseDragedRoutedEventArgs : RoutedEventArgs
    {
        public Point Origin { get; set; }

        public Point Target { get; set; }

        public MouseDragedRoutedEventArgs(RoutedEvent routedEvent, Point origin, Point traget)
            : base(routedEvent)
        {

            this.Origin = origin;
            this.Target = traget;
        }
    }

    public class MouseDragStartedRoutedEventArgs : RoutedEventArgs
    {
        public Point Origin { get; set; }

        public MouseDragStartedRoutedEventArgs(RoutedEvent routeEvent, Point origin)
            : base(routeEvent)
        {
            this.Origin = origin;
        }
    }

    public class MouseSelectedRoutedEventArgs : RoutedEventArgs
    {
        public Rect Rect { get; set; }

        public MouseSelectedRoutedEventArgs(RoutedEvent routeEvent, Rect rect)
            : base(routeEvent)
        {
            this.Rect = rect;
        }
    }

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
