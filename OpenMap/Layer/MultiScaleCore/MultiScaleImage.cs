using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace OpenMap
{
    /// <summary>
    /// WPF counterpart for the Silverlight MultiScaleImage class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi")]
    public class MultiScaleImage : Canvas
    {
        /// <summary>
        /// Identifies the UseSprings dependency property.
        /// </summary>
        public static readonly DependencyProperty UseSpringsProperty = DependencyProperty.Register(
            "UseSprings",
            typeof(bool),
            typeof(MultiScaleImage),
            new PropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="SpringAnimationsMode"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SpringAnimationsModeProperty = DependencyProperty.Register("SpringAnimationsMode",
            typeof(SpringAnimationsMode),
            typeof(MultiScaleImage),
            new PropertyMetadata(SpringAnimationsMode.All));

        /// <summary>
        /// Identifies the MotionFinished routed event.
        /// </summary>
        public static readonly RoutedEvent MotionFinishedEvent = EventManager.RegisterRoutedEvent(
            "MotionFinished",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(MultiScaleImage));

        /// <summary>
        /// Identifies the ViewportChanged routed event.
        /// </summary>
        public static readonly RoutedEvent ViewportChangedEvent = EventManager.RegisterRoutedEvent(
            "ViewportChanged",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(MultiScaleImage));

        /// <summary>
        /// Spring animation duration in seconds.
        /// </summary>
        private const double ZoomingAnimationDuration = 0.4d;
        private const double PanningAnimationDuration = 0.2d;
        private const double FollowForce = 0.95;
        private const double DragValue = 0.78;
        private const double ArrangeTilesInterval = 150;

        /// <summary>
        /// Identifies the InternalViewportWidth dependency property.
        /// </summary>
        private static readonly DependencyProperty InternalViewportWidthProperty = DependencyProperty.Register(
            "InternalViewportWidth",
            typeof(double),
            typeof(MultiScaleImage),
            new PropertyMetadata(new PropertyChangedCallback(InternalViewportWidthChangedHandler)));

        /// <summary>
        /// Identifies the InternalViewportOrigin dependency property.
        /// </summary>
        private static readonly DependencyProperty InternalViewportOriginProperty = DependencyProperty.Register(
            "InternalViewportOrigin",
            typeof(Point),
            typeof(MultiScaleImage),
            new PropertyMetadata(new PropertyChangedCallback(InternalViewportOriginChangedHandler)));

        private double imageLeft = 0;
        private double imageTop = 0;

        private MultiScaleTileSource multiScaleTileSource;
        private double viewportWidth;
        private Point viewportOrigin;

        private Canvas mainCanvas;
        private ScaleTransform scaleTransform;
        private TranslateTransform translateTransform;

        private bool isUseAnimation;

        private int zoomLevelShift;
        private double zoomLevel;
        private int requestZoomLevel;
        private double scaleFactor;
        private Size oldSize;

        private SplineDoubleKeyFrame animationSplineKeyFrame;

        private bool animationDriverAttached;

        private double frameInterval;
        private TimeSpan lastRendered;
        private bool animationStarted;
        private double animationTime;
        private double animationDuration;
        private bool zoomAnimation;

        private double startViewportWidth;
        private Point startViewportOrigin;
        private double lastViewportWidth;
        private Point lastViewportOrigin;

        private double animatorVieportWidth;
        private Point animatorVieportOrigin;
        private Vector velocityVector;
        private double pixelFactor;
        private bool pendingRaiseMotionFinished;

        private DispatcherTimer arrangeTimer;
        private bool arrangeInvalidated;
        private bool unloaded;
        private HashSet<TileSource> usedTiles = new HashSet<TileSource>();
        private HashSet<TileSource> unusedTiles;

        /// <summary>
        /// Initializes static members of the MultiScaleImage class.
        /// </summary>
        static MultiScaleImage()
        {
            MaxDownloadersCount = 16;
        }

        /// <summary>
        /// Initializes a new instance of the MultiScaleImage class.
        /// </summary>
        public MultiScaleImage()
        {
            this.ClipToBounds = true;
            this.SnapsToDevicePixels = true;

            this.scaleTransform = new ScaleTransform();
            this.scaleTransform.ScaleX = this.scaleTransform.ScaleY = 1;
            this.translateTransform = new TranslateTransform();
            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(this.scaleTransform);
            transformGroup.Children.Add(this.translateTransform);

            this.mainCanvas = new Canvas();
            this.mainCanvas.RenderTransform = transformGroup;
            this.Children.Add(this.mainCanvas);

            this.Loaded += this.MultiScaleImage_Loaded;
            this.Unloaded += this.MultiScaleImage_Unloaded;

            this.SizeChanged += this.MultiScaleImage_SizeChanged;
            this.IsVisibleChanged += this.MultiScaleImage_IsVisibleChanged;

            this.CreateSplineKeyFrame();
        }

        private delegate bool ValidateDelegate(TileDownloader downloader);

        /// <summary>
        /// Occurs when opening of image succeeded.
        /// </summary>
        public event RoutedEventHandler ImageOpenSucceeded;

        /// <summary>
        /// Occurs when zoom or pan animation ends.
        /// </summary>
        public event RoutedEventHandler MotionFinished
        {
            add
            {
                this.AddHandler(MotionFinishedEvent, value);
            }
            remove
            {
                this.RemoveHandler(MotionFinishedEvent, value);
            }
        }

        /// <summary>
        /// Occurs when the viewport is changed.
        /// </summary>
        public event RoutedEventHandler ViewportChanged
        {
            add
            {
                this.AddHandler(ViewportChangedEvent, value);
            }
            remove
            {
                this.RemoveHandler(ViewportChangedEvent, value);
            }
        }

        /// <summary>
        /// Maximum tile downloaders count.
        /// </summary>
        public static int MaxDownloadersCount
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the viewport origin.
        /// </summary>
        public Point ViewportOrigin
        {
            get
            {
                return this.InternalViewportOrigin;
            }
            set
            {
                this.isUseAnimation = (this.viewportOrigin.X != 0 || this.viewportOrigin.Y != 0) && this.viewportWidth != 0;

                this.viewportOrigin = value;
                this.SetMotion(InternalViewportOriginProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the width of the viewport.
        /// </summary>
        public double ViewportWidth
        {
            get
            {
                return this.InternalViewportWidth > 0 ? this.InternalViewportWidth : 0;
            }
            set
            {
                this.isUseAnimation = this.viewportWidth != 0;

                this.viewportWidth = value;
                this.SetPixelFactor();

                this.SetMotion(InternalViewportWidthProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether spring animations should be used.
        /// </summary>
        public bool UseSprings
        {
            get
            {
                return (bool)this.GetValue(MultiScaleImage.UseSpringsProperty);
            }
            set
            {
                this.SetValue(MultiScaleImage.UseSpringsProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets mode of animation.
        /// </summary>
        public SpringAnimationsMode SpringAnimationsMode
        {
            get
            {
                return (SpringAnimationsMode)this.GetValue(SpringAnimationsModeProperty);
            }

            set
            {
                this.SetValue(SpringAnimationsModeProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the tile source.
        /// </summary>
        public MultiScaleTileSource Source
        {
            get
            {
                return this.multiScaleTileSource as MultiScaleTileSource;
            }

            set
            {
                this.OnSourceChanged(this.multiScaleTileSource, value);
            }
        }

        internal Size TileSize
        {
            get;
            private set;
        }

        private double InternalViewportWidth
        {
            get
            {
                double width = (double)GetValue(MultiScaleImage.InternalViewportWidthProperty);
                if (width <= 0)
                {
                    width = this.viewportWidth;
                }

                return width;
            }
        }

        private Point InternalViewportOrigin
        {
            get
            {
                return (Point)GetValue(MultiScaleImage.InternalViewportOriginProperty);
            }
        }

        internal void RenderViewport()
        {
            if (this.multiScaleTileSource != null && this.viewportWidth > 0 && this.ActualWidth > 0)
            {
                this.SetViewportOrigin();
            }
        }

        internal void SetViewportOrigin()
        {
            this.ArrangeImage();

            var eventArgs = new RoutedEventArgs(ViewportChangedEvent);
            this.RaiseEvent(eventArgs);
        }

        /// <summary>
        /// Called to arrange and size the content.
        /// </summary>
        /// <param name="arrangeSize">The computed size that is used to arrange the content.</param>
        /// <returns>The calculated size.</returns>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            Size size = base.ArrangeOverride(arrangeSize);
            this.Arrange();

            return size;
        }

        private static void InternalViewportWidthChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiScaleImage image = d as MultiScaleImage;
            if (image != null && image.Source != null)
            {
                image.RenderViewport();
            }
        }

        private static void InternalViewportOriginChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiScaleImage image = d as MultiScaleImage;
            if (image != null && image.Source != null)
            {
                image.SetViewportOrigin();
            }
        }

        private static bool EqualPoints(Point pointA, Point pointB)
        {
            return Math.Round(pointA.X, 15) == Math.Round(pointB.X, 15)
                && Math.Round(pointA.Y, 15) == Math.Round(pointB.Y, 15);
        }

        private static bool EqualDoubles(double a, double b)
        {
            return Math.Round(a, 15) == Math.Round(b, 15);
        }

        private void OnSourceChanged(MultiScaleTileSource oldValue, MultiScaleTileSource newValue)
        {
            if (oldValue != null)
            {
                oldValue.TilesAvailable -= this.OnTilesAvailable;
            }

            if (newValue != null)
            {
                this.zoomLevelShift = (int)Math.Round(Math.Log(newValue.TileWidth, 2));
                this.TileSize = new Size(newValue.TileWidth, newValue.TileHeight);

                newValue.TilesAvailable += this.OnTilesAvailable;
            }

            this.multiScaleTileSource = newValue;

            this.RenderViewport();
        }

        private void ArrangeImage()
        {
            if (this.multiScaleTileSource != null && this.viewportWidth > 0 && this.ActualWidth > 0)
            {
                this.InvalidateArrange();
            }
        }

        private void Arrange()
        {
            MultiScaleTileSource tileSource = this.multiScaleTileSource;
            double internalViewportWidth = (double)this.GetValue(InternalViewportWidthProperty);
            if (tileSource != null && !(tileSource is EmptyTileMapSource) && internalViewportWidth > 0)
            {
                MultiscaleImageViewport state = new MultiscaleImageViewport()
                {
                    ViewportOrigin = this.viewportOrigin,
                    ViewportWidth = this.viewportWidth,
                    ActualWidth = this.ActualWidth,
                    ActualHeight = this.ActualHeight
                };

                tileSource.ProcessTilesDownload(state);

                this.CalculateImageLocation();
                this.ArrangeTiles();
            }
        }

        private void CalculateImageLocation()
        {
            Point internalViewportOrigin = this.ViewportOrigin;
            double internalViewportWidth = this.ViewportWidth;
            double ratio = this.ActualWidth / internalViewportWidth;

            this.imageLeft = -internalViewportOrigin.X * ratio;
            this.imageTop = -internalViewportOrigin.Y * ratio;

            if (EqualPoints(internalViewportOrigin, this.viewportOrigin) && EqualDoubles(internalViewportWidth, this.viewportWidth))
            {
                this.imageLeft = Math.Round(this.imageLeft);
                this.imageTop = Math.Round(this.imageTop);
            }

            double zoom = ratio / this.TileSize.Width;
            this.zoomLevel = zoom;

            zoom = Math.Log(zoom, 2d);
            int currentZoom = (int)Math.Round(zoom);

            this.requestZoomLevel = this.zoomLevelShift + currentZoom;
            this.scaleFactor = Math.Pow(2, zoom - currentZoom);
        }

        private void ArrangeTiles()
        {
            MultiScaleTileSource tileSource = this.multiScaleTileSource;
            if (this.ActualWidth > 0 && tileSource != null)
            {
                Rect imageRect = this.GetImageBounds();
                bool empty = imageRect.IsEmpty;
                if (!empty)
                {
                    this.unusedTiles = this.usedTiles;
                    this.usedTiles = new HashSet<TileSource>();

                    if (this.RenderSize != this.oldSize)
                    {
                        this.oldSize = this.RenderSize;

                        foreach (MapTile tile in this.mainCanvas.Children)
                        {
                            tile.Source = null;
                        }

                        this.mainCanvas.Children.Clear();
                        this.CreateTiles(tileSource);
                    }

                    this.SetTileSources(imageRect, tileSource);

                    foreach (TileSource source in this.unusedTiles)
                    {
                        source.IsUsed = false;
                        source.ClearBitmap();
                    }

                    this.unusedTiles.Clear();
                }

                this.SetTransform(empty);
            }
        }

        private void SetTransform(bool empty)
        {
            double tileWidth = this.TileSize.Width * this.scaleFactor;
            double tileHeight = this.TileSize.Height * this.scaleFactor;

            double x = this.imageLeft > 0 || empty ? this.imageLeft : (this.imageLeft % tileWidth);
            double y = this.imageTop > 0 || empty ? this.imageTop : (this.imageTop % tileHeight);

            this.translateTransform.X = x;
            this.translateTransform.Y = y;

            this.scaleTransform.ScaleX = this.scaleTransform.ScaleY = this.scaleFactor;
        }

        private void CreateTiles(MultiScaleTileSource tileSource)
        {
            int endTileX = (int)Math.Ceiling(this.ActualWidth / this.TileSize.Width);
            int endTileY = (int)Math.Ceiling(this.ActualHeight / this.TileSize.Height);

            int minTileNumber = (endTileX + 1) * (endTileY + 1);
            tileSource.SetMinTileNumber(minTileNumber);

            endTileX <<= 1;
            endTileY <<= 1;

            double top = 0;
            for (int tileY = 0; tileY <= endTileY; tileY++)
            {
                double left = 0;

                for (int tileX = 0; tileX <= endTileX; tileX++)
                {
                    MapTile tile = new MapTile(this.TileSize, new Point(left, top));
                    tile.RelativeTileId = new TileId(this.requestZoomLevel, tileX, tileY);
                    this.mainCanvas.Children.Add(tile);

                    left += this.TileSize.Width;
                }

                top += this.TileSize.Height;
            }
        }

        private Rect GetImageBounds()
        {
            double multiplier = this.zoomLevel;
            double imageWidth = this.TileSize.Width * multiplier;
            double imageHeight = this.TileSize.Height * multiplier;

            Rect imageRect = new Rect(0, 0, imageWidth, imageHeight);
            Rect viewportRect = new Rect(-this.imageLeft, -this.imageTop, this.ActualWidth, this.ActualHeight);
            imageRect.Intersect(viewportRect);

            return imageRect;
        }

        private void SetTileSources(Rect imageRect, MultiScaleTileSource source)
        {
            double tileWidth = this.TileSize.Width * this.scaleFactor;
            double tileHeight = this.TileSize.Height * this.scaleFactor;

            int startTileX = (int)(imageRect.X / tileWidth);
            int startTileY = (int)(imageRect.Y / tileHeight);
            int endTileX = (int)Math.Ceiling(imageRect.Right / tileWidth) - 1;
            int endTileY = (int)Math.Ceiling(imageRect.Bottom / tileWidth) - 1;
            foreach (MapTile tile in this.mainCanvas.Children)
            {
                int tileX = startTileX + tile.RelativeTileId.X;
                int tileY = startTileY + tile.RelativeTileId.Y;
                if (tileX > endTileX || tileY > endTileY)
                {
                    if (tile.Source != null)
                    {
                        tile.Source = null;
                    }

                    continue;
                }

                TileId tileId = new TileId(this.requestZoomLevel, tileX, tileY);
                this.SetTileSource(tile, tileId, source);
            }
        }

        private void SetTileSource(MapTile tile, TileId tileId, MultiScaleTileSource source)
        {
            bool refresh = false;
            if (!tileId.Equals(tile.TileId))
            {
                tile.TileId = tileId;
                refresh = true;
            }

            while (true)
            {
                TileSource tileSource = source.GetTileSource(tileId);
                if (tileSource != null)
                {
                    if (tile.Source != tileSource || refresh)
                    {
                        tile.Source = tileSource;
                    }

                    if (tileSource.IsUsed)
                    {
                        this.usedTiles.Add(tileSource);
                        this.unusedTiles.Remove(tileSource);

                        break;
                    }
                    else
                    {
                        tileSource = null;
                    }
                }

                if (tileSource == null)
                {
                    tileId = this.ReduceTileId(tileId);
                    if (tileId == null)
                    {
                        tile.Source = null;
                        break;
                    }
                }
            }
        }

        private TileId ReduceTileId(TileId tileId)
        {
            TileId result = null;
            int level = tileId.Level - 1;
            if (level > this.zoomLevelShift)
            {
                result = new TileId(level, tileId.X >> 1, tileId.Y >> 1);
            }

            return result;
        }

        private void OnTilesAvailable(object sender, EventArgs e)
        {
            this.arrangeInvalidated = true;
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            DispatcherTimer timer = sender as DispatcherTimer;
            if (timer != null)
            {
                if (this.unloaded)
                {
                    this.arrangeTimer = null;
                    timer.Stop();
                    timer.Tick -= this.OnTimerTick;
                    MultiScaleTileSource source = this.multiScaleTileSource;
                    if (source != null)
                    {
                        source.StopDownload();
                    }
                }

                if (this.arrangeInvalidated)
                {
                    this.arrangeInvalidated = false;
                    this.InvalidateArrange();
                }
            }
        }

        private void MultiScaleImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width != 0)
            {
                this.SetPixelFactor();
                this.RenderViewport();
                if (this.pendingRaiseMotionFinished)
                {
                    this.pendingRaiseMotionFinished = false;
                    this.RaiseMotionFinishedEvent();
                }
            }
        }

        private void MultiScaleImage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                this.RenderViewport();
            }
        }

        private void SetMotion(DependencyProperty property, object value)
        {
            if (!this.UseSprings || !this.isUseAnimation)
            {
                this.SetValue(property, value);

                if (this.ActualWidth > 0)
                {
                    this.RaiseMotionFinishedEvent();
                }
                else
                {
                    this.pendingRaiseMotionFinished = true;
                }
            }
            else if (!this.animationDriverAttached)
            {
                this.animationDriverAttached = true;
                CompositionTarget.Rendering += this.PostRenderingAnimator;
            }
        }

        private void SetPixelFactor()
        {
            double pixelWidth = this.viewportWidth / this.ActualWidth;
            this.pixelFactor = Math.Sqrt((pixelWidth * pixelWidth) + (pixelWidth * pixelWidth));
        }

        private void MultiScaleImage_Loaded(object sender, RoutedEventArgs e)
        {
            this.unloaded = false;

            if (this.ImageOpenSucceeded != null)
            {
                this.ImageOpenSucceeded(this, new RoutedEventArgs());
            }

            if (this.arrangeTimer == null)
            {
                this.arrangeInvalidated = true;
                DispatcherTimer timer = new DispatcherTimer()
                {
                    Interval = TimeSpan.FromMilliseconds(ArrangeTilesInterval)
                };

                this.arrangeTimer = timer;
                timer.Tick += this.OnTimerTick;
                timer.Start();
            }
        }

        private void MultiScaleImage_Unloaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer timer = this.arrangeTimer;
            if (timer != null)
            {
                timer.Stop();
                timer.Start();
                this.unloaded = true;
            }
        }

        private void PostRenderingAnimator(object sender, EventArgs e)
        {
            RenderingEventArgs renderingArgs = (RenderingEventArgs)e;
            TimeSpan renderingTime = renderingArgs.RenderingTime;
            if (!this.animationStarted)
            {
                this.lastRendered = renderingTime;
            }

            this.frameInterval = (renderingTime - this.lastRendered).TotalSeconds;
            this.lastRendered = renderingTime;

            if (this.IsAnimatorEnabled())
            {
                this.RenderingAnimation();
            }
        }

        private void RenderingAnimation()
        {
            double targetWidth = this.viewportWidth;
            Point targetOrigin = this.viewportOrigin;

            this.animatorVieportWidth = (double)this.GetValue(InternalViewportWidthProperty);
            this.animatorVieportOrigin = (Point)this.GetValue(InternalViewportOriginProperty);

            System.Console.WriteLine("RenderingAnimation   " + this.animatorVieportOrigin);
            this.frameInterval = this.frameInterval < 0 ? 0 : this.frameInterval;

            this.zoomAnimation = !EqualDoubles(this.animatorVieportWidth, targetWidth);
            if (this.zoomAnimation || !EqualPoints(this.animatorVieportOrigin, targetOrigin) || !this.animationStarted)
            {
                if (!this.animationStarted)
                {
                    this.InitializeAnimation(this.animatorVieportWidth, this.animatorVieportOrigin, targetWidth, targetOrigin);
                }

                this.animationDuration = this.zoomAnimation ? ZoomingAnimationDuration : PanningAnimationDuration;
                if (!this.zoomAnimation && this.SnapsToDevicePixels)
                {
                    this.SnapsToDevicePixels = false;
                }
                else if (!this.SnapsToDevicePixels)
                {
                    this.SnapsToDevicePixels = true;
                }

                this.ContinueAnimation(targetWidth, targetOrigin);
            }
            else if (this.animationStarted && this.animationTime >= this.animationDuration)
            {
                this.CompleteAnimation();
            }

            this.animationTime += this.frameInterval;
        }

        private void DetachAnimationDriver()
        {
            this.animationDriverAttached = false;
            CompositionTarget.Rendering -= this.PostRenderingAnimator;
        }

        private bool IsAnimatorEnabled()
        {
            if (this.isUseAnimation && this.UseSprings)
            {
                return true;
            }

            if (this.animationStarted)
            {
                this.SetAndCompleteAnimation();
            }

            return false;
        }

        private void InitializeAnimation(double width, Point origin, double targetWidth, Point targetOrigin)
        {
            // change animation status
            this.animationStarted = true;

            this.animationTime = this.frameInterval;
            this.startViewportWidth = width;
            this.startViewportOrigin = origin;
            this.lastViewportWidth = targetWidth;
            this.lastViewportOrigin = targetOrigin;
            this.velocityVector = new Vector(0, 0);
        }

        private void ContinueAnimation(double targetWidth, Point targetOrigin)
        {
            // viewport changed during animation
            if (this.lastViewportWidth != targetWidth
            || this.lastViewportOrigin != targetOrigin)
            {
                this.startViewportWidth = this.animatorVieportWidth;
                this.startViewportOrigin = this.animatorVieportOrigin;

                this.lastViewportWidth = targetWidth;
                this.lastViewportOrigin = targetOrigin;

                // change animation time
                this.animationTime = this.frameInterval;
            }

            if (this.zoomAnimation)
            {
                if (this.SpringAnimationsMode == SpringAnimationsMode.Panning && this.animationTime == 0)
                {
                    this.SetAndCompleteAnimation();
                }
                else
                {
                    this.ContinueSplineAnimation(targetWidth, targetOrigin);
                }
            }
            else
            {
                if (this.SpringAnimationsMode == SpringAnimationsMode.Zooming)
                {
                    this.SetAndCompleteAnimation();
                }
                else
                {
                    this.ContinueVelocityTypeAnimation(targetOrigin);
                }
            }
        }

        private void SetAndCompleteAnimation()
        {
            this.SetValue(InternalViewportWidthProperty, this.viewportWidth);
            this.SetValue(InternalViewportOriginProperty, this.viewportOrigin);

            this.CompleteAnimation();
        }

        private void ContinueVelocityTypeAnimation(Point targetOrigin)
        {
            if (this.frameInterval > 0)
            {
                Vector originVector = targetOrigin - this.animatorVieportOrigin;
                this.velocityVector += originVector * FollowForce;
                this.velocityVector *= DragValue;

                Vector vector = this.velocityVector * this.frameInterval;
                this.animatorVieportOrigin += vector;
                if (originVector.Length < vector.Length
                || originVector.Length < this.pixelFactor)
                {
                    this.animatorVieportOrigin = targetOrigin;
                }

                this.SetValue(InternalViewportOriginProperty, this.animatorVieportOrigin);
            }
        }

        private void ContinueSplineAnimation(double targetWidth, Point targetOrigin)
        {
            double progress = this.animationTime / this.animationDuration;
            progress = progress > 1d ? 1d : progress;

            double animationValue = this.animationSplineKeyFrame.InterpolateValue(0, progress);
            double x = this.startViewportOrigin.X + ((targetOrigin.X - this.startViewportOrigin.X) * animationValue);
            double y = this.startViewportOrigin.Y + ((targetOrigin.Y - this.startViewportOrigin.Y) * animationValue);
            Point newOrigin = new Point(x, y);
            if (EqualPoints(newOrigin, targetOrigin))
            {
                newOrigin = targetOrigin;
            }

            if (this.zoomAnimation)
            {
                double newWidth = this.startViewportWidth + ((targetWidth - this.startViewportWidth) * animationValue);
                if (EqualDoubles(newWidth, targetWidth))
                {
                    newWidth = targetWidth;
                }

                if (this.animatorVieportWidth != newWidth)
                {
                    this.SetValue(InternalViewportWidthProperty, newWidth);
                }
            }

            this.SetValue(InternalViewportOriginProperty, newOrigin);
        }

        private void CompleteAnimation()
        {
            this.DetachAnimationDriver();

            this.animationStarted = false;
            this.SnapsToDevicePixels = true;
            this.RaiseMotionFinishedEvent();
        }

        private void RaiseMotionFinishedEvent()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                RoutedEventArgs eventArgs = new RoutedEventArgs(MotionFinishedEvent);
                this.RaiseEvent(eventArgs);
            }));
        }

        private void CreateSplineKeyFrame()
        {
            this.animationSplineKeyFrame = new SplineDoubleKeyFrame()
            {
                Value = 1d,
                KeySpline = new KeySpline(0.2, 0.8, 0.3, 0.9)
            };

            this.animationSplineKeyFrame.Freeze();
        }
    }
}
