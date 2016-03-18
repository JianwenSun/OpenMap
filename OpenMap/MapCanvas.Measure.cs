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
        private void MeasureItems()
        {
            foreach (UIElement element in this.Children)
            {
                element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            }
        }
    }
}
