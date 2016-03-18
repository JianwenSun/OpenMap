using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenMap
{
    public class CoordinateService : ICoordinateService
    {
        public ISpatialReference SpatialReference
        {
            get
            {
                return this.Map.SpatialReference;
            }
        }

        public Map Map { get; }

        public CoordinateService(Map map)
        {
            this.Map = map;
        }

        public Point GeographicToLogical(Location location)
        {
            return this.SpatialReference.GeographicToLogical(location);
        }

        public Location LogicalToGeographic(Point point)
        {
            return this.SpatialReference.LogicalToGeographic(point);
        }

        public Point LogicalToPixel(Point logical)
        {
            double scaleFactor = this.Map.MultiScaleImage.ActualWidth;
            Point logicalOrigin = this.Map.MultiScaleImage.ViewportOrigin;
            double viewLogicalWidth = this.Map.MultiScaleImage.ViewportWidth;

            Point offset = new Point(-logicalOrigin.X / viewLogicalWidth,
                -logicalOrigin.Y / viewLogicalWidth);

            double zoomFactor = 1 / viewLogicalWidth;

            Point interimPoint = new Point(offset.X + (zoomFactor * logical.X),
                offset.Y + (zoomFactor * logical.Y));
            return new Point(interimPoint.X * scaleFactor, interimPoint.Y * scaleFactor);
        }

        public Point PixelToLogical(Point pixel)
        {
            Point origin = this.Map.MultiScaleImage.ViewportOrigin;
            double width = this.Map.MultiScaleImage.ActualWidth;
            origin = new Point(Math.Round(origin.X * width) / width, Math.Round(origin.Y * width) / width);

            Point offset = this.Map.LogicalOrigin;

            Size viewLogicalSize = new Size(this.Map.viewportWidth,
                    this.Map.viewportWidth * this.Map.MultiScaleImage.ActualHeight / this.Map.MultiScaleImage.ActualWidth);

            double pixelFactorX = viewLogicalSize.Width / this.Map.MultiScaleImage.ActualWidth;
            double pixelFactorY = viewLogicalSize.Height / this.Map.MultiScaleImage.ActualHeight;
            return new Point((pixel.X * pixelFactorX) + offset.X, (pixel.Y * pixelFactorY) + offset.Y);
        }

        public Point GeographicToPixel(Location location)
        {
            return this.LogicalToPixel(this.GeographicToLogical(location));
        }

        public Location PixelToGeographic(Point logical)
        {
            return this.LogicalToGeographic(this.PixelToLogical(logical));
        }
    }
}
