using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenMap
{
    public static class ViewportHelper
    {
        public static void SetZoomLevel(Map map, int zoomLevel)
        {
            double viewportPixelWidth = map.Provider.TileSize.Width * Math.Pow(2.0, zoomLevel);
            map.viewportPixelWidth = viewportPixelWidth;
            UpdateLogicalOrigin(map, map.Center);
            double viewportWidth = map.ActualWidth / viewportPixelWidth;
            map.viewportWidth = viewportWidth;
            map.MultiScaleImage.ViewportWidth = viewportWidth;
        }

        public static void UpdateLogicalOrigin(Map map, Location geoCenter)
        {
            Point center = map.SpatialReference.GeographicToLogical(geoCenter);
            double targetWidth = map.ActualWidth / map.Provider.TileSize.Width / Math.Pow(2.0, map.ZoomLevel);
            double targetHeight = map.ActualHeight / map.Provider.TileSize.Height / Math.Pow(2.0, map.ZoomLevel);
            center.X -= targetWidth / 2;
            center.Y -= targetHeight / 2;
            map.LogicalOrigin = center;
            map.MultiScaleImage.ViewportOrigin = center;
        }
    }
}
