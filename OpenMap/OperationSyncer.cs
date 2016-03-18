using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OpenMap
{
    public class OperationSyncer
    {
        public Map Map { get; set; }

        public bool Syncing { get; set; }

        public OperationSyncer() { }
       
        public OperationSyncer(Map map)
        {
            this.Map = map;
        }

        public void Start(Map map)
        {
            this.Stop();
            this.Map = map;

            this.Syncing = true;
            this.Map.AddHandler(MouseControl.MouseDragStartedEvent, new MouseDragStartedRoutedEventHandler(this.OnDragStarted));
            this.Map.AddHandler(MouseControl.MouseDragedEvent, new MouseDragedRoutedEventHandler(this.OnDraged));
            this.Map.AddHandler(MouseControl.MouseClickEvent, new MouseClickRoutedEventHandler(this.OnClick));
            this.Map.AddHandler(MouseControl.MouseSelectedEvent, new MouseSelectedRoutedEventHandler(this.OnSelected));
            this.Map.AddHandler(MouseControl.MouseWheelEvent, new MouseWheelRoutedEventHandler(this.OnWheel));
        }

        public void Stop()
        {
            if (this.Map == null) return;
            this.Map.RemoveHandler(MouseControl.MouseDragStartedEvent, new MouseDragStartedRoutedEventHandler(this.OnDragStarted));
            this.Map.RemoveHandler(MouseControl.MouseDragedEvent, new MouseDragedRoutedEventHandler(this.OnDraged));
            this.Map.RemoveHandler(MouseControl.MouseClickEvent, new MouseClickRoutedEventHandler(this.OnClick));
            this.Map.RemoveHandler(MouseControl.MouseSelectedEvent, new MouseSelectedRoutedEventHandler(this.OnSelected));
            this.Map.RemoveHandler(MouseControl.MouseWheelEvent, new MouseWheelRoutedEventHandler(this.OnWheel));
            this.Syncing = false;
        }

        private void OnDragStarted(object sender, MouseDragStartedRoutedEventArgs e)
        {
            System.Console.WriteLine("Drag Started");
        }

        private void OnDraged(object sender, MouseDragedRoutedEventArgs e)
        {
            System.Console.WriteLine("Draged");
        }

        private void OnClick(object sender, MouseClickRoutedEventArgs e)
        {
            System.Console.WriteLine("Click");
        }
        private void OnSelected(object sender, MouseSelectedRoutedEventArgs e)
        {
            System.Console.WriteLine("Selected");
        }

        private void OnWheel(object sender, MouseWheelRoutedEventArgs e)
        {
            this.Map.OnMouseWheel(e.Delta, e.Origin);
            System.Console.WriteLine("Wheel");
        }
    }
}
