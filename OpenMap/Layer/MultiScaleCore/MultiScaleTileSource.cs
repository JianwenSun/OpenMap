using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace OpenMap
{
    /// <summary>
    /// Represents the tile source of the multi scale image control.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi")]
    public abstract class MultiScaleTileSource : DependencyObject
    {
        internal const int DefaultTileCacheSize = 10 * 1024 * 1024;

        private static readonly object authenticationRequestSynchronizer = new object();

        private TilesDownloadManager downloadManager;

        private int levelCount;
        private int maxTileCacheSize = DefaultTileCacheSize;
        private int minTileNumber;
        private int tileWidth, tileHeight;

        /// <summary>
        /// Initializes a new instance of the MultiScaleTileSource class.
        /// </summary>
        /// <param name="imageWidth">Not used.</param>
        /// <param name="imageHeight">Not used.</param>
        /// <param name="tileWidth">Tile width.</param>
        /// <param name="tileHeight">Tile height.</param>
        /// <param name="tileOverlap">Not used.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "tileOverlap"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "imageHeight")]
        protected MultiScaleTileSource(int imageWidth, int imageHeight, int tileWidth, int tileHeight, int tileOverlap)
        {
            if (tileWidth < 0 || tileHeight < 0)
            {
                throw new ArgumentException("Invalid argument");
            }

            this.tileWidth = tileWidth;
            this.tileHeight = tileHeight;
            this.levelCount = (int)Math.Log(imageWidth / tileWidth, 2);
            this.RequestCacheLevel = RequestCacheLevel.CacheIfAvailable;
        }

        internal event EventHandler TilesAvailable;

        /// <summary>
        /// Gets or sets credentials for downloading tiles.
        /// </summary>
        public ICredentials RequestCredentials
        {
            get;
            set;
        }

        internal int LevelCount
        {
            get
            {
                return this.levelCount;
            }
        }

        internal int TileWidth
        {
            get
            {
                return this.tileWidth;
            }
        }

        internal int TileHeight
        {
            get
            {
                return this.tileHeight;
            }
        }

        /// <summary>
        /// Request cache level for downloading tiles.
        /// </summary>
        internal RequestCacheLevel RequestCacheLevel
        {
            get;
            set;
        }

        /// <summary>
        /// Returned a streams that is loaded from Uri inside the MultiScaleImage.
        /// </summary>
        /// <param name="tileLevel">Level of the tile.</param>
        /// <param name="tilePositionX">X-matrix coordinate of the tile.</param>
        /// <param name="tilePositionY">Y-matrix coordinate of the tile.</param>
        /// <param name="uri">Uri.</param>
        /// <param name="expires">DateTime of expires.</param>
        /// <param name="tileBody">Tile body.</param>
        public virtual void CacheTile(int tileLevel, int tilePositionX, int tilePositionY, Uri uri, DateTime expires, byte[] tileBody)
        {
        }

        /// <summary>
        /// Invalidates specified tile layers.
        /// </summary>
        /// <param name="tileLevel">Tile level.</param>
        /// <param name="tilePositionX">X position of the tile.</param>
        /// <param name="tilePositionY">Y position of the tile.</param>
        /// <param name="tileLayer">Layer of the tile.</param>
        public virtual void InvalidateTileLayer(int tileLevel, int tilePositionX, int tilePositionY, int tileLayer)
        {
            TilesDownloadManager manager = this.downloadManager;
            if (manager != null)
            {
                TileId tileId = new TileId(tileLevel, tilePositionX, tilePositionY);
                manager.RemoveRequest(tileId);
                manager.Refresh();
            }
        }

        /// <summary>
        /// Sets maximal size of tile cache.
        /// </summary>
        /// <param name="bytes">The size in bytes.</param>
        public void SetTileCacheSize(int bytes)
        {
            this.maxTileCacheSize = bytes;
            TilesDownloadManager downloader = this.downloadManager;
            if (downloader != null)
            {
                downloader.MaxTileCacheSize = this.maxTileCacheSize;
            }
        }

        /// <summary>
        /// The MultiScaleImage control calls this method to get the URI's for the base layer and all Overlays.
        /// </summary>
        /// <param name="tileLevel">The MSI tile Level.</param>
        /// <param name="tilePositionX">The number of tiles from the left (0 based) for this tile level.</param>
        /// <param name="tilePositionY">The number of tiles from the top (0 based) for this tile level.</param>
        /// <returns>Uri.</returns>
        internal List<object> GetTileLayers(int tileLevel, int tilePositionX, int tilePositionY)
        {
            List<object> tileImageLayerSources = new List<object>();
            this.GetTileLayers(tileLevel, tilePositionX, tilePositionY, tileImageLayerSources);

            return tileImageLayerSources;
        }

        internal void ProcessTilesDownload(MultiscaleImageViewport state)
        {
            if (!(this is EmptyTileMapSource))
            {
                TilesDownloadManager manager = null;
                if (this.downloadManager == null)
                {
                    TilesDownloadManager downloader = new TilesDownloadManager(this);
                    downloader.MaxTileCacheSize = this.maxTileCacheSize;
                    downloader.MinTileNumber = this.minTileNumber;
                    this.downloadManager = downloader;
                    downloader.StartDownload();
                }

                manager = this.downloadManager;

                state.TileWidth = this.tileWidth;
                state.TileHeight = this.tileHeight;

                if (manager != null)
                {
                    manager.DoProcess(state);
                }
            }
        }

        internal TileSource GetTileSource(TileId tileId)
        {
            TileSource tileSource = null;
            TilesDownloadManager manager = this.downloadManager;
            if (manager != null)
            {
                tileSource = manager.GetTileSource(tileId);
            }

            return tileSource;
        }

        internal void DownloadTile(TileId tileId)
        {
            IList<object> objects = null;

            using (ManualResetEvent complete = new ManualResetEvent(false))
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    objects = this.GetTileLayers(tileId.Level, tileId.X, tileId.Y);
                    complete.Set();
                }));

                complete.WaitOne();
            }

            if (objects.Count > 0)
            {
                Uri uri = objects[0] as Uri;
                Stream stream = objects[0] as Stream;
                if (uri != null)
                {
                    TileDownloadState e = new TileDownloadState()
                    {
                        TileId = tileId,
                        Uri = uri,
                        Callback = this.AssyncImageCacheReader
                    };

                    this.GetCachedTileAsync(tileId.Level, tileId.X, tileId.Y, e.RaiseEvent);
                }
                else if (stream != null)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        TileDownloader tile = new TileDownloader(tileId, stream, this.AssyncImageReader);
                        tile.StartDownload();
                    }));
                }
                else
                {
                    TileDownloadState e = new TileDownloadState()
                    {
                        TileId = tileId
                    };

                    this.UpdateTile(e);
                }
            }
        }

        internal void SetMinTileNumber(int tilesCount)
        {
            int minNumber = 0;
            int counter = tilesCount;
            for (int i = this.LevelCount; i > 0; i--)
            {
                minNumber += counter;
                counter /= 2;
                if (counter < 4)
                {
                    minNumber += 4 * i;
                    break;
                }
            }

            this.minTileNumber = minNumber + 1;

            TilesDownloadManager downloader = this.downloadManager;
            if (downloader != null)
            {
                downloader.MinTileNumber = this.minTileNumber;
            }
        }

        internal void StopDownload()
        {
            TilesDownloadManager manager = this.downloadManager;
            if (manager != null)
            {
                this.downloadManager = null;
                manager.StopDownload();
            }
        }

        /// <summary>
        /// Gets a collection of the URIs that comprise the Deep Zoom image.
        /// </summary>
        /// <param name="tileLevel">Level of the tile.</param>
        /// <param name="tilePositionX">X-matrix coordinate of the tile.</param>
        /// <param name="tilePositionY">Y-matrix coordinate of the tile.</param>
        /// <param name="tileImageLayerSources">Source of the tile image layer, which is a collection of URIs.</param>
        protected abstract void GetTileLayers(int tileLevel, int tilePositionX, int tilePositionY, IList<object> tileImageLayerSources);

        /// <summary>
        /// Requests the cached tile.
        /// </summary>
        /// <param name="tileLevel">Level of the tile.</param>
        /// <param name="tilePositionX">X-matrix coordinate of the tile.</param>
        /// <param name="tilePositionY">Y-matrix coordinate of the tile.</param>
        /// <param name="callback">Callback which should be called to return tile if it is available or null.</param>
        protected virtual void GetCachedTileAsync(int tileLevel, int tilePositionX, int tilePositionY, Action<byte[]> callback)
        {
            callback(null);
        }

        /// <summary>
        /// When overridden in a derived class, is invoked when downloading of a tile image via HTTP gets the Unauthorized state in the HttpWebResponse.
        /// The method should provide credentials using the RequestCredentials property.
        /// </summary>
        /// <param name="uri">The Request Uri.</param>
        /// <param name="type">Authorization type.</param>
        /// <param name="attributes">Authorization attributes like "realm" for Basic HTTP authentication.</param>
        protected virtual void OnHttpAuthenticationRequired(Uri uri, string type, Dictionary<string, string> attributes)
        {
        }

        private void AssyncImageCacheReader(TileDownloadState e)
        {
            TilesDownloadManager manager = this.downloadManager;
            if (manager != null)
            {
                if (e.TileBody == null)
                {
                    TileDownloader downloader = new TileDownloader(e.TileId, e.Uri, this.AssyncImageReader, manager.ValidateTile);
                    downloader.StartDownload(this.RequestCacheLevel, this.RequestCredentials, this.HttpUnauthorizedHandler);
                }
                else
                {
                    this.UpdateTile(e);
                }
            }
        }

        private ICredentials HttpUnauthorizedHandler(Uri requestUri, string authenticationType, Dictionary<string, string> authenticationAttributes, NetworkCredential usedCredentials)
        {
            lock (authenticationRequestSynchronizer)
            {
                ICredentials credentials = this.RequestCredentials;
                if (credentials == null || usedCredentials == credentials.GetCredential(requestUri, authenticationType))
                {
                    using (ManualResetEvent handler = new ManualResetEvent(false))
                    {
                        requestUri = new Uri(requestUri.AbsoluteUri);
                        this.Dispatcher.BeginInvoke(new Action<Uri, string, Dictionary<string, string>, ManualResetEvent>((uri, type, attributes, resetEvent) =>
                        {
                            this.OnHttpAuthenticationRequired(uri, type, attributes);
                            resetEvent.Set();
                        }),
                        requestUri,
                        authenticationType,
                        authenticationAttributes,
                        handler);

                        handler.WaitOne();
                    }
                }

                return this.RequestCredentials;
            }
        }

        private void AssyncImageReader(TileDownloader tileDownloader)
        {
            if (tileDownloader.Uri != null)
            {
                this.CacheTile(tileDownloader.TileId.Level, tileDownloader.TileId.X, tileDownloader.TileId.Y, tileDownloader.Uri, tileDownloader.Expires, tileDownloader.TileBody);
            }

            this.AssyncImageReaderComplete(tileDownloader);
        }

        private void AssyncImageReaderComplete(TileDownloader downloader)
        {
            TileDownloadState e = new TileDownloadState()
            {
                TileId = downloader.TileId,
                TileBody = downloader.TileBody
            };
            downloader.Dispose();

            this.UpdateTile(e);
        }

        private void UpdateTile(TileDownloadState e)
        {
            TilesDownloadManager manager = this.downloadManager;
            if (manager != null)
            {
                TileSource tileSource = new TileSource(e.TileId, e.TileBody);
                manager.UpdateRequest(tileSource);
                if (e.TileBody != null && manager.ValidateTile(e.TileId))
                {
                    this.OnTileAvailable();
                }

                e.Dispose();
            }
        }

        private void OnTileAvailable()
        {
            EventHandler handler = this.TilesAvailable;
            if (handler != null)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    handler(this, null);
                }));
            }
        }
    }
}
