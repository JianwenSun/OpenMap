using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OpenMap
{
    public class MapCanvasItem
    {
        internal const int UnknownPosition = -10000;
        public Rect Rect { get; set; }
        public UIElement Item { get; set; }

        internal MapCanvas Canvas { get; set; }


        public void Capture()
        {
            FrameworkElement element = this.Item as FrameworkElement;
            if (element != null)
            {
                element.LayoutUpdated -= new EventHandler(this.ItemLayoutUpdated);
                element.LayoutUpdated += new EventHandler(this.ItemLayoutUpdated);
            }
        }

        private void ItemLayoutUpdated(object sender, EventArgs e)
        {
            if (this.Canvas != null)
            {
                FrameworkElement element = this.Item as FrameworkElement;
                if (element != null)
                {
                    element.LayoutUpdated -= new EventHandler(this.ItemLayoutUpdated);
                }

                Point point = this.CalculateItemPosition();

                if (point.X == UnknownPosition && point.Y == UnknownPosition)
                {
                    if (element != null)
                    {
                        element.LayoutUpdated += new EventHandler(this.ItemLayoutUpdated);
                    }
                }
                this.Canvas.SetItemPosition(this, point);
            }
        }

        internal Point CalculateItemPosition()
        {
            FrameworkElement objectItem = this.Item as FrameworkElement;

            Point point = new Point(UnknownPosition, UnknownPosition);

            Location location = Location.Empty;
            ContentPresenter presenter = this.Item as ContentPresenter;
            if (presenter != null)
            {
                
            }
            else if (objectItem != null)
            {
                location = MapLayer.GetLocation(objectItem);
            }

            if (objectItem != null
                && objectItem.DesiredSize.Width > 0
                && objectItem.DesiredSize.Height > 0)
            {
               
            }

            return point;
        }
    }
}
