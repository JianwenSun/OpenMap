using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OpenMap
{
    public partial class MapCanvas : Panel
    {
        public Map Map { get; internal set; }

        public MapCanvas()
        {
            this.LoadCompeleted(this.OnLoadCompeleted);
        }

        protected virtual void OnLoadCompeleted(object sender, RoutedEventArgs e)
        {
            this.Refresh();
        }

        protected virtual MapCanvasItem PrepareItem(UIElement item)
        {
            return new MapCanvasItem() { Item = item, Canvas = this };
        }

        public void Refresh()
        {
            this.CollectItems();
            this.MeasureItems();
            this.ArrangeItems();
        }
    }
}
