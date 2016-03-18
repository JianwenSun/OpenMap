
using System;
using System.Windows;
using System.Windows.Controls;

namespace OpenMap
{
    public abstract class MapLayer : ItemsControl, ILayer
    {
        /// <summary>
		/// Identifies the Location dependency property.
		/// </summary>
		public static readonly DependencyProperty LocationProperty = DependencyProperty.RegisterAttached(
            "Location",
            typeof(Location),
            typeof(MapLayer),
            new PropertyMetadata(Location.Empty, new PropertyChangedCallback(LocationChangedHandler)));

        public Map MapControl
        {
            get;
            set;
        }

        /// <summary>
		/// Gets value of the attachable Location property.
		/// </summary>
		/// <param name="element">Element to get value of the property from.</param>
		/// <returns>Value of the Location property.</returns>
		public static Location GetLocation(DependencyObject element)
        {
            return (Location)element.GetValue(LocationProperty);
        }

        /// <summary>
		/// Sets value of the attachable Location property.
		/// </summary>
		/// <param name="element">Element to set value of the property to.</param>
		/// <param name="value">Geographical location of the element.</param>
		public static void SetLocation(DependencyObject element, Location value)
        {
            element.SetValue(LocationProperty, value);
        }

        #region Public Method


        #endregion

        #region Private PropertyChangedCallback

        /// <summary>
        /// Location property changed callback. 
        /// </summary>
        /// <param name="source">Source.</param>
        /// <param name="eventArgs">Event Args.</param>
        private static void LocationChangedHandler(DependencyObject source, DependencyPropertyChangedEventArgs eventArgs)
        {

        }

        #endregion
    }
}
