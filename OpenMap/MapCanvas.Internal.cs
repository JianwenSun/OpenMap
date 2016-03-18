using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OpenMap
{
    /// <summary>
    /// internal
    /// </summary>
    public partial class MapCanvas
    {
        private Dictionary<int, MapCanvasItem> internalItems = new Dictionary<int, MapCanvasItem>();

        private void CollectItems()
        {
            foreach (UIElement element in this.Children)
            {
                MapCanvasItem item = null;
                int hash = element.GetHashCode();
                if (!this.internalItems.TryGetValue(hash, out item))
                {
                    item = this.PrepareItem(element);
                    this.internalItems.Add(hash, item);
                }
            }
        }
    }
}
