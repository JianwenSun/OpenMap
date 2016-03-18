using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenMap
{
    public static class Extends
    {
        public static bool IsUnkown(this Point point)
        {
            return point.X == MapCanvasItem.UnknownPosition && point.Y == MapCanvasItem.UnknownPosition;
        }
    }
}
