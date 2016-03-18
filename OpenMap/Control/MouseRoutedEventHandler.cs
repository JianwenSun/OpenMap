using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenMap
{
    public delegate void MouseClickRoutedEventHandler(object sender, MouseClickRoutedEventArgs e);
    public delegate void MouseDragedRoutedEventHandler(object sender, MouseDragedRoutedEventArgs e);
    public delegate void MouseSelectedRoutedEventHandler(object sender, MouseSelectedRoutedEventArgs e);
    public delegate void MouseWheelRoutedEventHandler(object sender, MouseWheelRoutedEventArgs e);
    public delegate void MouseDragStartedRoutedEventHandler(object sender, MouseDragStartedRoutedEventArgs e);
}
