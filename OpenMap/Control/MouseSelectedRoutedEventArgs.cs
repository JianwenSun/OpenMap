using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenMap
{
    public class MouseSelectedRoutedEventArgs : RoutedEventArgs
    {
        public Rect Rect { get; set; }

        public MouseSelectedRoutedEventArgs(RoutedEvent routeEvent, Rect rect)
            : base(routeEvent)
        {
            this.Rect = rect;
        }
    }
}
