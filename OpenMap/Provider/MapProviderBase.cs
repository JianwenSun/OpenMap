using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenMap
{
    /// <summary>
	/// Base class for all map providers. Every map provider can use 1 type of the tile sources. 
	/// I.e. TiledProvider can use TiledMapSources only and ImageProvider can use ImageMapSource only.
	/// </summary>
	public class MapProviderBase :
        Freezable, IDisposable
    {
        /// <summary>
        /// Identifies the <see cref="Opacity"/> Opacity dependency property.
        /// </summary>
        public static readonly DependencyProperty OpacityProperty = DependencyProperty.Register(
            "Opacity",
            typeof(double),
            typeof(MapProviderBase),
            new PropertyMetadata(1d, OpacityChanged));

        /// <summary>
        /// Initializes a new instance of the MapProviderBase class.
        /// </summary>
        protected MapProviderBase()
        {
            this.MapSources = new Dictionary<string, IMapSource>();
        }

        /// <summary>
        /// Event occurs when spatial reference is ready to use.
        /// </summary>
        public event SpatialReferenceEventHandler SpatialReferenceChanged;

        /// <summary>
        /// Gets or sets the opacity factor.
        /// </summary>
        public double Opacity
        {
            get
            {
                return (double)this.GetValue(OpacityProperty);
            }
            set
            {
                this.SetValue(OpacityProperty, value);
            }
        }

        /// <summary>
        /// Gets ids of the supported sources.
        /// </summary>
        public string[] SupportedSources
        {
            get
            {
                string[] supported = new string[this.MapSources.Keys.Count];
                this.MapSources.Keys.CopyTo(supported, 0);
                return supported;
            }
        }

        /// <summary>
        /// Gets spatial reference of the current map source.
        /// </summary>
        public virtual ISpatialReference SpatialReference
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets current source.
        /// </summary>
        internal IMapSource CurrentSource
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets size of the tile.
        /// </summary>
        internal Size TileSize
        {
            get
            {
                if (this.CurrentSource != null)
                {
                    TiledMapSource tiledSource = this.CurrentSource as TiledMapSource;
                    if (tiledSource != null)
                    {
                        return new Size(tiledSource.TileWidth, tiledSource.TileHeight);
                    }
                }

                return new Size(256, 256);
            }
        }

        /// <summary>
        /// Gets dictionary of the available map sources.
        /// </summary>
        protected Dictionary<string, IMapSource> MapSources
        {
            get;
            private set;
        }

        /// <summary>
        /// Releases the resources used by the current instance of the MapProviderBase class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Set specific map source to clone of map provider.
        /// </summary>
        /// <param name="clone">Clone of map provider.</param>
        /// <param name="uniqueId">Id of map source.</param>
        public virtual void SetMapSourceToClone(MapProviderBase clone, string uniqueId)
        {
            IMapSource source = clone.CurrentSource;
            if (source == null || source.UniqueId != uniqueId)
            {
                clone.SetMapSource(uniqueId);
            }
        }

        /// <summary>
        /// Force map provider to use specific map source.
        /// </summary>
        /// <param name="uniqueId">Unique ID of the map source.</param>
        public void SetMapSource(string uniqueId)
        {
            IMapSource source = null;
            if (this.MapSources.TryGetValue(uniqueId, out source))
            {
                string oldSourceId = this.CurrentSource != null ? this.CurrentSource.UniqueId : null;
                this.CurrentSource = source;
            }
        }

        /// <summary>
		/// Raise SpatialReferenceChanged event.
		/// </summary>
		internal void RaiseSpatialReferenceChanged(SpatialReferenceEventArgs e)
        {
            if (this.SpatialReferenceChanged != null)
                this.SpatialReferenceChanged(this, e);
        }


        /// <summary>
        /// When implemented in a derived class, creates a new instance of the Freezable derived class.
        /// </summary>
        /// <returns>The new instance.</returns>
        protected override Freezable CreateInstanceCore()
		{
			return new MapProviderBase();
		}

        /// <summary>
        /// Called before map provider switch to another map source.
        /// </summary>
        /// <param name="source">Map source which will be used.</param>
        protected virtual bool OnPreviewSourceChange(IMapSource source)
        {
            return true;
        }

        /// <summary>
        /// Called after map provider switch to another map source.
        /// </summary>
        /// <param name="source">Map source is used.</param>
        protected virtual void OnPreviewSourceChanged(IMapSource source)
        {
        }

        private static void OpacityChanged(DependencyObject source, DependencyPropertyChangedEventArgs eventArgs)
        {
            MapProviderBase provider = source as MapProviderBase;
            if (provider != null)
            {
                provider.OnOpacityChanged((double)eventArgs.OldValue, (double)eventArgs.NewValue);
            }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "oldOpacity")]
        private void OnOpacityChanged(double oldOpacity, double newOpacity)
        {
            foreach (IMapSource source in this.MapSources.Values)
            {
                source.Opacity = newOpacity;
            }
        }

        /// <summary>
        /// Called by the Dispose() and Finalize() methods to release the unmanaged 
        /// resources used by the current instance of the MapProviderBase class.
        /// </summary>
        /// <param name="disposing">True to release unmanaged and managed resources;
        /// false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
