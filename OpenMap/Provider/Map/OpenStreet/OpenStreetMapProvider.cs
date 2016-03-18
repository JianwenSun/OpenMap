using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenMap
{
    /// <summary>
	/// Represents the Open Street Map Provider class.
	/// </summary>
	public class OpenStreetMapProvider : TiledProvider
    {
        /// <summary>
        /// Identifies the Layer dependency property.
        /// </summary>
        public static readonly DependencyProperty LayerProperty = DependencyProperty.Register(
            "Layer",
            typeof(TOpenStreetMapLayer),
            typeof(OpenStreetMapProvider),
            new PropertyMetadata(TOpenStreetMapLayer.Standard, LayerChanged, LayerCoerceChanged));

        private Dictionary<TOpenStreetMapLayer, string> mapModeSource = new Dictionary<TOpenStreetMapLayer, string>();
        private MercatorProjection projection = new MercatorProjection();

        /// <summary>
        /// Initializes a new instance of the OpenStreetMapProvider class.
        /// </summary>
        public OpenStreetMapProvider()
            : base()
        {
            TiledMapSource source = new OpenStreetMapnikSource();
            this.MapSources.Add(source.UniqueId, source);
            this.mapModeSource[TOpenStreetMapLayer.Standard] = source.UniqueId;
        }

        /// <summary>
        /// Gets or sets map layer.
        /// </summary>
        public TOpenStreetMapLayer Layer
        {
            get
            {
                return (TOpenStreetMapLayer)this.GetValue(LayerProperty);
            }
            set
            {
                this.SetValue(LayerProperty, value);
            }
        }

        /// <summary>
        /// Returns the SpatialReference for the map provider.
        /// </summary>
        public override ISpatialReference SpatialReference
        {
            get
            {
                return this.projection;
            }
        }

        private static void LayerChanged(DependencyObject source, DependencyPropertyChangedEventArgs eventArgs)
        {
            OpenStreetMapProvider provider = source as OpenStreetMapProvider;
            if (provider != null)
            {
                provider.OnLayerChanged((TOpenStreetMapLayer)eventArgs.NewValue);
            }
        }

        private static object LayerCoerceChanged(DependencyObject source, object baseValue)
        {
            OpenStreetMapProvider provider = source as OpenStreetMapProvider;
            if (provider != null)
            {
                provider.OnLayerChanged((TOpenStreetMapLayer)baseValue);
            }

            return (TOpenStreetMapLayer)baseValue;
        }
        

        /// <summary>
        /// MapModeChanged handler.
        /// </summary>
        /// <param name="newLayer">New layer.</param>
        private void OnLayerChanged(TOpenStreetMapLayer newLayer)
        {
            this.SetMapSource(this.mapModeSource[newLayer]);
        }
    }
}
