using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OpenMap
{
    public static class UIElementExtensions
    {
        /// <summary>
        /// FrameworkElement Load compeleted.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="onLoaded"></param>
        public static void LoadCompeleted(this FrameworkElement element, RoutedEventHandler onLoaded)
        {
            if (element == null || onLoaded == null)
                throw new ArgumentNullException();

            RoutedEventHandler wrappedOnLoaded = null;
            wrappedOnLoaded = delegate (object sender, RoutedEventArgs e)
            {
                element.Loaded -= wrappedOnLoaded;
                onLoaded(sender, e);
            };
            element.Loaded += wrappedOnLoaded;
        }
    }
}
