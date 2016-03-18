using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OpenMap
{
    public partial class MapCanvas : Panel
    {
        /// <summary>
        /// Arrange item in canvas.
        /// </summary>
        /// <param name="item">Item to arrange.</param>
        internal void ArrangeItem(UIElement element)
        {
            MapCanvasItem item = null;
            this.internalItems.TryGetValue(element.GetHashCode(), out item);

            if (item != null)
            {
                Location location = MapLayer.GetLocation(element);
                if (location.IsEmpty) return;

                ICoordinateService coordinateService = CoordinateServiceProvider.GetService(this.Map);
                Point pixel = coordinateService.GeographicToPixel(location);
                this.SetItemPosition(item, pixel);
            }
        }

        internal void ArrangeItems()
        {
            foreach (var item in this.Children)
            {
                this.ArrangeItem(item as UIElement);
            }
        }

        #region private

        internal void SetItemPosition(MapCanvasItem canvasItem, Point point)
        {
            if (!MapCanvas.VerifyPointIsValid(point))
                return;

            double width = canvasItem.Item.DesiredSize.Width;
            double height = canvasItem.Item.DesiredSize.Height;

            if (!double.IsNaN(width) && !double.IsNaN(height)
                && !double.IsInfinity(width) && !double.IsInfinity(height)
                && (width > 0 || height > 0))
            {
                canvasItem.Item.Arrange(new Rect(point, new Size(Math.Ceiling(width), Math.Ceiling(height))));
            }
        }

        private static bool VerifyPointIsValid(Point point)
        {
            return !double.IsNaN(point.X) &&
                   !double.IsInfinity(point.X) &&
                   !double.IsNaN(point.Y) &&
                   !double.IsInfinity(point.Y);
        }

        #endregion
    }
}
