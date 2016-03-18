using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenMap
{
    public interface ICoordinateService
    {
        Map Map { get; }

        ISpatialReference SpatialReference { get; }

        Point GeographicToLogical(Location location);
        Location LogicalToGeographic(Point point);

        Point LogicalToPixel(Point logical);
        Point PixelToLogical(Point logical);

        Point GeographicToPixel(Location location);
        Location PixelToGeographic(Point logical);
    }
}
