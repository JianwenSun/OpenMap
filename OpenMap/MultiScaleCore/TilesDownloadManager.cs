using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace OpenMap
{
    internal class TilesDownloadManager
    {
        private const int MaxRemovedTiles = 3;

        private MultiscaleImageViewport parameters;
        private ManualResetEvent processRequest = new ManualResetEvent(false);
        private bool abort = false;
        private Dictionary<TileId, TileSource> requests = new Dictionary<TileId, TileSource>(TileId.EqualityComparer);
        private Collection<TileId> requestList = new Collection<TileId>();
        private object requestsSyncronizer = new object();
        private WeakReference multiScaleTileSource;
        private bool processStart;
        private bool hasParameters;
        private bool refreshInvoked;

        private int zoomLevelShift;
        private int endLevel;
        private int startTileX;
        private int startTileY;
        private int endTileX;
        private int endTileY;

        private int tileCacheSize = 0;

        internal TilesDownloadManager(MultiScaleTileSource source)
        {
            this.multiScaleTileSource = new WeakReference(source);
            this.MaxTileCacheSize = MultiScaleTileSource.DefaultTileCacheSize;
        }

        public int MaxTileCacheSize
        {
            get;
            set;
        }

        public int MinTileNumber
        {
            get;
            set;
        }

        private MultiScaleTileSource Source
        {
            get
            {
                WeakReference source = this.multiScaleTileSource;
                if (source != null
                    && source.IsAlive)
                {
                    return source.Target as MultiScaleTileSource;
                }
                else
                {
                    return null;
                }
            }
        }

        internal void StartDownload()
        {
            Thread thread = new Thread(new ThreadStart(this.DownloaderThread));
            thread.IsBackground = true;
            thread.Start();
        }

        internal void DoProcess(MultiscaleImageViewport state)
        {
            this.parameters = state;
            this.hasParameters = true;
            this.processStart = true;
            this.processRequest.Set();
        }

        internal void Refresh()
        {
            if (!this.refreshInvoked && this.hasParameters)
            {
                MultiScaleTileSource source = this.Source;
                if (source != null && !source.Dispatcher.HasShutdownStarted)
                {
                    this.refreshInvoked = true;
                    source.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.refreshInvoked = false;
                        this.processStart = true;
                        this.processRequest.Set();
                    }));
                }
            }
        }

        internal void StopDownload()
        {
            this.abort = true;
            this.processStart = true;
            this.processRequest.Set();
        }

        internal TileSource GetTileSource(TileId tileId)
        {
            TileSource tileSource = null;
            if (!this.abort)
            {
                lock (this.requestsSyncronizer)
                {
                    if (this.requests.ContainsKey(tileId))
                    {
                        tileSource = this.requests[tileId] as TileSource;
                    }
                }
            }

            return tileSource;
        }

        internal void UpdateRequest(TileSource e)
        {
            if (!this.abort)
            {
                lock (this.requestsSyncronizer)
                {
                    TileId key = e.TileId;
                    if (this.requests.ContainsKey(key))
                    {
                        TileSource source = this.requests[key];
                        if (source != null)
                        {
                            this.tileCacheSize -= this.requests[key].Size;
                        }

                        this.tileCacheSize += e.Size;
                        this.requests[key] = e;
                    }
                }

                this.processStart = true;
                ManualResetEvent processManager = this.processRequest;
                if (processManager != null)
                {
                    processManager.Set();
                }
            }
        }

        internal bool ValidateTile(TileId tileId)
        {
            if (!this.abort)
            {
                MultiscaleImageViewport viewport = this.parameters;
                if (viewport.ActualWidth > 0 && viewport.ViewportWidth > 0)
                {
                    int tileZoomLevelShift = (int)Math.Round(Math.Log(viewport.TileWidth, 2));
                    int tileLevel = tileId.Level - tileZoomLevelShift;
                    double zoom = viewport.ActualWidth / viewport.TileWidth / viewport.ViewportWidth;
                    int tileEndLevel = (int)Math.Ceiling(Math.Log(zoom, 2d));

                    if (tileLevel <= tileEndLevel)
                    {
                        double scale = Math.Pow(2d, Math.Log(zoom, 2d) - tileLevel);

                        MultiScaleTileSource tileSource = this.Source;
                        if (tileSource != null)
                        {
                            double width = tileSource.TileWidth * scale;
                            double height = tileSource.TileHeight * scale;
                            double x = tileId.X * width;
                            double y = tileId.Y * height;

                            double imageLeft = -viewport.ActualWidth * viewport.ViewportOrigin.X / viewport.ViewportWidth;
                            double imageTop = -viewport.ActualWidth * viewport.ViewportOrigin.Y / viewport.ViewportWidth;

                            Rect tileBounds = new Rect(x + imageLeft, y + imageTop, width, height);
                            Rect imageBounds = new Rect(0, 0, viewport.ActualWidth, viewport.ActualHeight);
                            imageBounds.Intersect(tileBounds);
                            if (!imageBounds.IsEmpty)
                            {
                                return true;
                            }
                        }
                    }
                }

                this.RemoveRequest(tileId);
            }

            return false;
        }

        internal void RemoveRequest(TileId key)
        {
            lock (this.requestsSyncronizer)
            {
                if (this.requests.ContainsKey(key))
                {
                    this.RemoveTile(key);
                }
            }
        }

        private static Rect GetImageBounds(double zoom, MultiscaleImageViewport viewport)
        {
            double imageLeft = -viewport.ActualWidth * viewport.ViewportOrigin.X / viewport.ViewportWidth;
            double imageTop = -viewport.ActualWidth * viewport.ViewportOrigin.Y / viewport.ViewportWidth;

            double imageWidth = zoom * viewport.TileWidth;
            double imageHeight = zoom * viewport.TileHeight;

            Rect imageRect = new Rect(0, 0, imageWidth, imageHeight);
            Rect viewportRect = new Rect(-imageLeft, -imageTop, viewport.ActualWidth, viewport.ActualHeight);
            imageRect.Intersect(viewportRect);

            return imageRect;
        }

        private void RemoveTile(TileId key)
        {
            TileSource tileSource = this.requests[key];
            if (tileSource != null)
            {
                this.tileCacheSize -= tileSource.Size;
                tileSource.Dispose();
            }

            this.requestList.Remove(key);
            this.requests.Remove(key);
        }

        private void DownloaderThread()
        {
            while (!this.abort)
            {
                WaitHandle[] waitHandles = new WaitHandle[1];
                waitHandles[0] = this.processRequest;
                WaitHandle.WaitAll(waitHandles);

                if (this.abort)
                {
                    break;
                }

                this.processStart = false;
                this.processRequest.Reset();

                this.Process();
            }

            this.Dispose();
        }

        private void Dispose()
        {
            ManualResetEvent process = this.processRequest;
            if (process != null)
            {
                this.processRequest = null;
                ((IDisposable)process).Dispose();
            }

            this.multiScaleTileSource.Target = null;
            this.multiScaleTileSource = null;

            this.RemoveL1Cache();
        }

        private void RemoveL1Cache()
        {
            lock (this.requestsSyncronizer)
            {
                TileId[] removed = this.requestList.ToArray();
                foreach (TileId key in removed)
                {
                    this.RemoveTile(key);
                }
            }
        }

        private void Process()
        {
            MultiscaleImageViewport viewport = this.parameters;
            if (viewport.ActualWidth > 0 && viewport.ViewportWidth > 0)
            {
                this.zoomLevelShift = (int)Math.Round(Math.Log(viewport.TileWidth, 2));
                double zoom = viewport.ActualWidth / viewport.TileWidth / viewport.ViewportWidth;
                double zoomLevel = Math.Log(zoom, 2d);
                double currentZoom = Math.Round(zoomLevel);
                double scaleFactor = Math.Pow(2, zoomLevel - currentZoom);
                double tileWidth = viewport.TileWidth * scaleFactor;
                double tileHeight = viewport.TileHeight * scaleFactor;

                Rect imageRect = TilesDownloadManager.GetImageBounds(zoom, viewport);
                if (!imageRect.IsEmpty)
                {
                    this.endLevel = (int)Math.Ceiling(Math.Log(zoom, 2d));
                    this.startTileX = (int)(imageRect.X / tileWidth);
                    this.startTileY = (int)(imageRect.Y / tileHeight);
                    this.endTileX = (int)Math.Ceiling(imageRect.Right / tileWidth) - 1;
                    this.endTileY = (int)Math.Ceiling(imageRect.Bottom / tileHeight) - 1;

                    int endLevel3 = this.endLevel - 2;
                    endLevel3 = endLevel3 >= 0 ? endLevel3 : 0;
                    this.RequestTop3Levels(endLevel3);
                    this.RequestOtherLevels(endLevel3);

                    this.UpdateTileChache();
                }
            }
        }

        private void RequestTop3Levels(int endLevel3)
        {
            for (int level = endLevel3; level <= this.endLevel; level++)
            {
                int requestLevel = level + this.zoomLevelShift;
                int shift = this.endLevel - level;
                int levelStartTileX = this.startTileX >> shift;
                int levelStartTileY = this.startTileY >> shift;
                int levelEndTileX = this.endTileX >> shift;
                int levelEndTileY = this.endTileY >> shift;
                for (int tileY = levelStartTileY; tileY <= levelEndTileY; tileY++)
                {
                    for (int tileX = levelStartTileX; tileX <= levelEndTileX; tileX++)
                    {
                        if (this.abort)
                        {
                            return;
                        }

                        this.RequestTile(requestLevel, tileX, tileY);
                    }
                }
            }
        }

        private void RequestOtherLevels(int endLevel3)
        {
            endLevel3--;
            if (endLevel3 >= 0)
            {
                for (int level = endLevel3; level >= 0; level--)
                {
                    int requestLevel = level + this.zoomLevelShift;
                    int shift = this.endLevel - level;
                    int levelStartTileX = this.startTileX >> shift;
                    int levelStartTileY = this.startTileY >> shift;
                    int levelEndTileX = this.endTileX >> shift;
                    int levelEndTileY = this.endTileY >> shift;
                    for (int tileY = levelStartTileY; tileY <= levelEndTileY; tileY++)
                    {
                        for (int tileX = levelStartTileX; tileX <= levelEndTileX; tileX++)
                        {
                            if (this.abort)
                            {
                                return;
                            }

                            bool run = this.RequestTile(requestLevel, tileX, tileY);
                            if (run && this.processStart)
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }

        private bool RequestTile(int requestLevel, int tileX, int tileY)
        {
            TileId key = new TileId(requestLevel, tileX, tileY);
            bool download = this.UpdateCachedTile(key);
            if (download)
            {
                MultiScaleTileSource source = this.Source;
                if (source != null)
                {
                    source.DownloadTile(key);
                }
            }

            return download;
        }

        private void UpdateTileChache()
        {
            int tilesCounter;
            lock (this.requestsSyncronizer)
            {
                tilesCounter = this.requestList.Count;
            }

            for (int i = 0;
                i < MaxRemovedTiles
                && tilesCounter > this.MinTileNumber
                && this.tileCacheSize > this.MaxTileCacheSize;
                i++)
            {
                lock (this.requestsSyncronizer)
                {
                    TileId removed = this.requestList[0];
                    TileSource tileSource = this.requests[removed];
                    if (tileSource != null && !tileSource.IsUsed)
                    {
                        this.tileCacheSize -= tileSource.Size;
                        tileSource.Dispose();

                        this.requestList.RemoveAt(0);
                        this.requests.Remove(removed);
                    }
                    else
                    {
                        this.requestList.Remove(removed);
                        this.requestList.Add(removed);
                        break;
                    }

                    tilesCounter = this.requestList.Count;
                }
            }
        }

        private bool UpdateCachedTile(TileId key)
        {
            bool download = false;
            lock (this.requestsSyncronizer)
            {
                if (!this.requests.ContainsKey(key))
                {
                    this.requestList.Add(key);
                    this.requests.Add(key, null);
                    download = true;
                }
                else
                {
                    this.requestList.Remove(key);
                    this.requestList.Add(key);
                }
            }

            return download;
        }
    }
}
