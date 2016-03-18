using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenMap
{
    /// <summary>
    /// Map source which provides tiles for the MultiScaleImage.
    /// </summary>
    public class TiledMapSource : MultiScaleTileSource, IMapSource
    {
        /// <summary>
        /// Identifies the <see cref="Opacity"/> Opacity dependency property.
        /// </summary>
        public static readonly DependencyProperty OpacityProperty = DependencyProperty.Register(
            "Opacity",
            typeof(double),
            typeof(TiledMapSource),
            new PropertyMetadata(1d));

        private List<TileInformation>[] nullTiles;

        private int tileLevelToZoomShift = 8;

        /// <summary>
        /// Initializes a new instance of the TiledMapSource class.
        /// </summary>
        /// <param name="minZoomLevel">Min zoom level.</param>
        /// <param name="maxZoomLevel">Max zoom level.</param>
        /// <param name="tileWidth">Width of the tile.</param>
        /// <param name="tileHeight">Height of the tile.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "maxZoomLevel+1")]
        protected TiledMapSource(int minZoomLevel, int maxZoomLevel, int tileWidth, int tileHeight)
            : base(
                   (int)Math.Pow(2, maxZoomLevel) * tileWidth,
                   (int)Math.Pow(2, maxZoomLevel) * tileHeight,
                   tileWidth,
                   tileHeight,
                   0)
        {
            this.UniqueId = this.GetType().FullName;

            this.tileLevelToZoomShift = (int)Math.Ceiling(Math.Log(this.TileWidth, 2));
            this.MinZoomLevel = minZoomLevel;
            this.MaxZoomLevel = maxZoomLevel;

            this.nullTiles = new List<TileInformation>[maxZoomLevel + 1];
            for (int i = 0; i < this.nullTiles.Length; i++)
            {
                this.nullTiles[i] = new List<TileInformation>();
            }
        }

        private delegate bool IsValidCacheUriDelegate();

        /// <summary>
        /// Occurs when initialization of the map source is completed.
        /// </summary>
        public event EventHandler InitializeCompleted;

        /// <summary>
        /// Gets or sets the CacheStorage property.
        /// </summary>
        public ICacheStorage CacheStorage
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets culture.
        /// </summary>
        public CultureInfo Culture
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the IsTileCachingEnabled property.
        /// </summary>
        public bool IsTileCachingEnabled
        {
            get;
            set;
        }

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
        /// Gets unique identifier of the map source.
        /// </summary>
        public string UniqueId
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets minimal zoom level.
        /// </summary>
        protected int MinZoomLevel
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets maximum zoom level.
        /// </summary>
        protected int MaxZoomLevel
        {
            get;
            private set;
        }

        /// <summary>
        /// The MultiScaleImage control calls this method to possible caching tiles by provider.
        /// </summary>
        /// <param name="tileLevel">The MSI tile Level.</param>
        /// <param name="tilePositionX">The number of tiles from the left (0 based) for this tile level.</param>
        /// <param name="tilePositionY">The number of tiles from the top (0 based) for this tile level.</param>
        /// <param name="uri">Uri.</param>
        /// <param name="expires">DateTime of expires.</param>
        /// <param name="tileBody">Tile body.</param>
        public override void CacheTile(int tileLevel, int tilePositionX, int tilePositionY, Uri uri, DateTime expires, byte[] tileBody)
        {
            if (this.IsTileCachingEnabled && this.CacheStorage != null)
            {
                string path = this.GetCachedTileName(tileLevel, tilePositionX, tilePositionY);
                if (!string.IsNullOrEmpty(path))
                {
                    bool valid = !this.Dispatcher.HasShutdownStarted
                    && (bool)this.Dispatcher.Invoke(new IsValidCacheUriDelegate(() =>
                    {
                        return this.IsValidCacheUri(tileLevel, tilePositionX, tilePositionY, uri);
                    }));

                    if (valid)
                    {
                        this.CacheStorage.Save(path, expires, tileBody);
                    }
                }
            }
        }

        /// <summary>
        /// Initialize map source.
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Indicates whether specified tile level is supported.
        /// </summary>
        /// <param name="level">Tile level.</param>
        /// <returns>true if tile level is supported. false otherwise.</returns>
        public virtual bool IsLevelSupported(int level)
        {
            return true;
        }

        /// <summary>
        /// Converts a tile level to a zoom level.
        /// </summary>
        /// <param name="tileLevelDetail">The tile level (2^n = pixel width).</param>
        /// <returns>The zoom level.</returns>
        protected virtual int ConvertTileToZoomLevel(int tileLevelDetail)
        {
            return tileLevelDetail - this.tileLevelToZoomShift;
        }

        /// <summary>
        /// Converts a zoom level to a tile level.
        /// </summary>
        /// <param name="zoomLevel">The zoom level.</param>
        /// <returns>The tile level.</returns>
        protected virtual int ConvertZoomToTileLevel(int zoomLevel)
        {
            return zoomLevel + this.tileLevelToZoomShift;
        }

        /// <summary>
        /// Generates cache tile name.
        /// </summary>
        /// <param name="tileLevel">Tile level.</param>
        /// <param name="tilePositionX">Tile X.</param>
        /// <param name="tilePositionY">Tile Y.</param>
        /// <returns>Cache tile name.</returns>
        protected virtual string GetCachedTileName(int tileLevel, int tilePositionX, int tilePositionY)
        {
            return this.UniqueId
                + "." + tileLevel.ToString()
                + "x" + tilePositionX.ToString()
                + "x" + tilePositionY.ToString();
        }

        /// <summary>
        /// Gets the image URI.
        /// </summary>
        /// <param name="tileLevel">Tile level.</param>
        /// <param name="tilePositionX">Tile X.</param>
        /// <param name="tilePositionY">Tile Y.</param>
        /// <returns>URI of image.</returns>
        protected virtual Stream GetCachedTile(int tileLevel, int tilePositionX, int tilePositionY)
        {
            return null;
        }

        /// <summary>
        /// Validates loaded uri to tile position.
        /// Should be overridden if the provider loads the same tile from different http addresses.
        /// </summary>
        /// <param name="tileLevel">The MSI tile Level.</param>
        /// <param name="tilePositionX">The number of tiles from the left (0 based) for this tile level.</param>
        /// <param name="tilePositionY">The number of tiles from the top (0 based) for this tile level.</param>
        /// <param name="uri">Uri.</param>
        /// <returns>True if the loaded uri is valid.</returns>
        protected virtual bool IsValidCacheUri(int tileLevel, int tilePositionX, int tilePositionY, Uri uri)
        {
            Uri tileUri = this.GetTile(tileLevel, tilePositionX, tilePositionY);

            return tileUri != null && uri.OriginalString == tileUri.OriginalString;
        }

        /// <summary>
        /// Requests the cached tile.
        /// </summary>
        /// <param name="tileLevel">Level of the tile.</param>
        /// <param name="tilePositionX">X-matrix coordinate of the tile.</param>
        /// <param name="tilePositionY">Y-matrix coordinate of the tile.</param>
        /// <param name="callback">Callback which should be called to return tile if it is available or null.</param>
        protected override void GetCachedTileAsync(int tileLevel, int tilePositionX, int tilePositionY, Action<byte[]> callback)
        {
            if (this.IsTileCachingEnabled && this.CacheStorage != null)
            {
                string path = this.GetCachedTileName(tileLevel, tilePositionX, tilePositionY);
                if (!string.IsNullOrEmpty(path))
                {
                    this.CacheStorage.LoadAsync(path, callback);
                }
            }
            else
            {
                callback(null);
            }
        }

        /// <summary>
        /// The MultiScaleImage control calls this method to get the URI's for the base layer and all Overlays.
        /// </summary>
        /// <param name="tileLevel">The MSI tile Level.</param>
        /// <param name="tilePositionX">The number of tiles from the left (zero based) for this tile level.</param>
        /// <param name="tilePositionY">The number of tiles from the top (zero based) for this tile level.</param>
        /// <param name="tileImageLayerSources">A reference to the object to add the layer and Overlay's URI's too.</param>
        protected override void GetTileLayers(int tileLevel, int tilePositionX, int tilePositionY, IList<object> tileImageLayerSources)
        {
            if (this.IsValidTileLevel(tileLevel))
            {
                object tile = this.GetCachedTile(tileLevel, tilePositionX, tilePositionY);
                if (tile == null)
                {
                    tile = this.GetTile(tileLevel, tilePositionX, tilePositionY);
                }

                tileImageLayerSources.Add(tile);

                if (tile == null)
                {
                    TileInformation info = new TileInformation()
                    {
                        X = tilePositionX,
                        Y = tilePositionY,
                        Layer = 0
                    };

                    this.nullTiles[this.ConvertTileToZoomLevel(tileLevel)].Add(info);
                }
            }
        }

        /// <summary>
        /// Gets the image URI.
        /// </summary>
        /// <param name="tileLevel">Tile level.</param>
        /// <param name="tilePositionX">Tile X.</param>
        /// <param name="tilePositionY">Tile Y.</param>
        /// <returns>URI of image.</returns>
        protected virtual Uri GetTile(int tileLevel, int tilePositionX, int tilePositionY)
        {
            return null;
        }

        /// <summary>
        /// Does the supplied tile Level fall within the range of valid levels.
        /// </summary>
        /// <param name="tileLevel">The proposed level.</param>
        /// <returns>True if it is valid else false.</returns>
        protected virtual bool IsValidTileLevel(int tileLevel)
        {
            int zoomLevel = this.ConvertTileToZoomLevel(tileLevel);
            return zoomLevel >= this.MinZoomLevel && zoomLevel <= this.MaxZoomLevel;
        }

        /// <summary>
        /// Invalidate tiles which have not been loaded yet.
        /// </summary>
        protected void InvalidateNullTiles()
        {
            for (int i = 0; i < this.nullTiles.Length; i++)
            {
                List<TileInformation> tileList = this.nullTiles[i];
                foreach (TileInformation info in tileList)
                {
                    this.InvalidateTileLayer(
                        this.ConvertZoomToTileLevel(i),
                        info.X,
                        info.Y,
                        info.Layer);
                }
                tileList.Clear();
            }
        }

        /// <summary>
        /// Raise InitializeCompleted event.
        /// </summary>
        protected void RaiseInitializeCompleted()
        {
            this.InvalidateNullTiles();

            if (this.InitializeCompleted != null)
            {
                this.InitializeCompleted(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raise InitializeCompleted event.
        /// </summary>
        [Obsolete("Use RaiseInitializeCompleted method instead.")]
        protected void RaiseIntializeCompleted()
        {
            this.RaiseInitializeCompleted();
        }
    }
}
