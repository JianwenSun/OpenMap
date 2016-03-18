using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace OpenMap
{
    internal static class ViewportHelper
    {
        public static void SetZoomLevel(Map map, int zoomLevel)
        {
            double viewportPixelWidth = map.Provider.TileSize.Width * Math.Pow(2.0, zoomLevel);
            map.viewportPixelWidth = viewportPixelWidth;
            SetLogicalOrigin(map, map.Center);
            double viewportWidth = map.MultiScaleImage.ActualWidth / viewportPixelWidth;
            map.viewportWidth = viewportWidth;
            map.MultiScaleImage.ViewportWidth = viewportWidth;
        }

        public static void SetLogicalOrigin(Map map, Location geoCenter)
        {
            Point center = map.SpatialReference.GeographicToLogical(geoCenter);
            double targetWidth = map.MultiScaleImage.ActualWidth / map.Provider.TileSize.Width / Math.Pow(2.0, map.ZoomLevel);
            double targetHeight = map.MultiScaleImage.ActualHeight / map.Provider.TileSize.Height / Math.Pow(2.0, map.ZoomLevel);
            center.X -= targetWidth / 2;
            center.Y -= targetHeight / 2;
            map.LogicalOrigin = center;
            map.MultiScaleImage.ViewportOrigin = center;
        }

        public static void SetZoomToPoint(Map map, Point point, int zoomAdjustment)
        {
            ICoordinateService coordinateService = CoordinateServiceProvider.GetService(map);
            Point center = map.SpatialReference.GeographicToLogical(map.Center);
            Location newCenter = map.Center;
            IMapSource mode = map.Provider.CurrentSource;
            Point sourceLogical = coordinateService.PixelToLogical(point);
            Location sourcePoint = coordinateService.PixelToGeographic(point);
            map.ZoomLevel += zoomAdjustment;
            Point currentLogical = coordinateService.PixelToLogical(point);
            Location currentPoint = coordinateService.PixelToGeographic(point);

            if (mode == map.Provider.CurrentSource)
            {
                Point shift = new Point(sourceLogical.X - currentLogical.X, sourceLogical.Y - currentLogical.Y);
                center.X += shift.X;
                center.Y += shift.Y;

                newCenter = map.SpatialReference.LogicalToGeographic(center);
            }
            else
            {
                if (map.Center.Longitude < sourcePoint.Longitude)
                    newCenter.Longitude = map.Center.Longitude + sourcePoint.Longitude - currentPoint.Longitude;
                else
                    newCenter.Longitude = map.Center.Longitude - currentPoint.Longitude + sourcePoint.Longitude;

                if (map.Center.Latitude < sourcePoint.Latitude)
                    newCenter.Latitude = map.Center.Latitude + sourcePoint.Latitude - currentPoint.Latitude;
                else
                    newCenter.Latitude = map.Center.Latitude - currentPoint.Latitude + sourcePoint.Latitude;
            }

            map.Center = newCenter;
        }

        public static void SetDrag(Map map, Point point, Point origin, Location mouseCenter)
        {
            MouseDragBehavior behavior = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift ?
                map.MouseShiftDragMode : map.MouseDragMode;

            switch (behavior)
            {
                case MouseDragBehavior.Drag:
                    {
                        ICoordinateService coordinateService = CoordinateServiceProvider.GetService(map);
                        Point pixelCenter = coordinateService.LogicalToPixel(map.SpatialReference.GeographicToLogical(mouseCenter));
                        pixelCenter.X += origin.X - point.X;
                        pixelCenter.Y += origin.Y - point.Y;

                        Point offset = map.MultiScaleImage.ViewportOrigin;
                        Size viewLogicalSize = new Size(map.MultiScaleImage.ViewportWidth,
                                map.MultiScaleImage.ViewportWidth * map.MultiScaleImage.ActualHeight / map.MultiScaleImage.ActualWidth);

                        double pixelFactorX = viewLogicalSize.Width / map.MultiScaleImage.ActualWidth;
                        double pixelFactorY = viewLogicalSize.Height / map.MultiScaleImage.ActualHeight;
                        Point logical = new Point((pixelCenter.X * pixelFactorX) + offset.X, (pixelCenter.Y * pixelFactorY) + offset.Y);
                        map.Center = coordinateService.LogicalToGeographic(logical);
                    }
                    break;

                case MouseDragBehavior.Select:
                    {
                        map.MouseControl.SelectBoxLocation = new Point(Math.Min(origin.X, point.X), Math.Min(origin.Y, point.Y));
                        var newSize = new Size(Math.Abs(origin.X - point.X), Math.Abs(origin.Y - point.Y));
                        map.MouseControl.SelectBoxSize = newSize;
                    }
                    break;
            }
        }

        public static bool SetMouseWheel(Map map, double delta, Point point)
        {
            if (map.MouseWheelMode != MouseWheelBehavior.None)
            {
                delta = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift ? -delta : delta;
                int zoomAdjustment = (int)Math.Round(delta);

                if (map.MouseWheelMode == MouseWheelBehavior.ZoomToPoint)
                {
                    map.SetZoomToPoint(point, zoomAdjustment);
                }
                else
                {
                    map.ZoomLevel += zoomAdjustment;
                }

                return true;
            }

            return false;
        }
    }
}
