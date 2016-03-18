using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenMap
{
    public class Map : ItemsControl
    {
        private const string MouseControlPartName = "PART_MouseControl";
        private const string MultiScaleImagePartName = "PART_MultiScaleImage";

        #region DependencyProperty

        /// <summary>
        /// Identifies the <see cref="Provider"/> Provider dependency property.
        /// </summary>
        public static readonly DependencyProperty ProviderProperty = DependencyProperty.Register(
            "Provider",
            typeof(MapProviderBase),
            typeof(Map),
            new PropertyMetadata(ProviderChanged));

        /// <summary>
		/// Identifies the <see cref="ZoomLevel"/> ZoomLevel dependency property.
		/// </summary>
		public static readonly DependencyProperty ZoomLevelProperty = DependencyProperty.Register(
            "ZoomLevel",
            typeof(int),
            typeof(Map),
            new PropertyMetadata(1, ZoomLevelChanged, ZoomLevelCoerced));

        /// <summary>
		/// Identifies the <see cref="MaxZoomLevel"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty MaxZoomLevelProperty = DependencyProperty.Register(
            "MaxZoomLevel",
            typeof(int),
            typeof(Map),
            new PropertyMetadata(20, ZoomLevelRangeChanged));

        /// <summary>
        /// Identifies the <see cref="MinZoomLevel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinZoomLevelProperty = DependencyProperty.Register(
            "MinZoomLevel",
            typeof(int),
            typeof(Map),
            new PropertyMetadata(1, ZoomLevelRangeChanged));

        /// <summary>
		/// Identifies the <see cref="Center"/> Center dependency property.
		/// </summary>
		public static readonly DependencyProperty CenterProperty = DependencyProperty.Register(
            "Center",
            typeof(Location),
            typeof(Map),
            new PropertyMetadata(new Location(), CenterChangedHandler, CenterCoerced));

        /// <summary>
        /// Sync with the mouse operation between mousecontrol map and multiscale image.
        /// </summary>
        public static readonly DependencyProperty OperationSyncerProperty = DependencyProperty.Register(
            "OperationSyncer",
            typeof(OperationSyncer),
            typeof(Map),
            new PropertyMetadata(null, OnOperationSyncerPropertyChanged));

        /// <summary>
		/// Identifies the MouseWheelMode dependency property.
		/// </summary>
		public static readonly DependencyProperty MouseWheelModeProperty = DependencyProperty.Register(
            "MouseWheelMode",
            typeof(MouseWheelBehavior),
            typeof(Map),
            new PropertyMetadata(MouseWheelBehavior.ZoomToPoint));

        /// <summary>
		/// Identifies the <see cref="MouseShiftDragMode"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty MouseShiftDragModeProperty = DependencyProperty.Register(
            "MouseShiftDragMode",
            typeof(MouseDragBehavior),
            typeof(Map),
            new PropertyMetadata(MouseDragBehavior.Select));

        /// <summary>
		/// Identifies the <see cref="MouseDragMode"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty MouseDragModeProperty = DependencyProperty.Register(
            "MouseDragMode",
            typeof(MouseDragBehavior),
            typeof(Map),
            new PropertyMetadata(MouseDragBehavior.Drag));

        #endregion

        #region Property
        /// <summary>
        /// Gets or sets zoom level.
        /// </summary>
        public int ZoomLevel
        {
            get
            {
                return (int)this.GetValue(ZoomLevelProperty);
            }

            set
            {
                this.SetValue(ZoomLevelProperty, value);
            }
        }

        /// <summary>
		/// Gets or sets maximum ZoomLevel.
		/// </summary>
		public int MaxZoomLevel
        {
            get
            {
                return (int)this.GetValue(MaxZoomLevelProperty);
            }
            set
            {
                this.SetValue(MaxZoomLevelProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets minimum ZoomLevel.
        /// </summary>
        public int MinZoomLevel
        {
            get
            {
                return (int)this.GetValue(MinZoomLevelProperty);
            }
            set
            {
                this.SetValue(MinZoomLevelProperty, value);
            }
        }

        /// <summary>
		/// Gets or sets center of the map.
		/// </summary>
		public Location Center
        {
            get
            {
                return (Location)this.GetValue(CenterProperty);
            }

            set
            {
                this.SetValue(CenterProperty, value);
            }
        }

        /// <summary>
        /// Operation sync between mousecontrol map and multiscaleiamge.
        /// </summary>
        public OperationSyncer OperationSyncer
        {
            get
            {
                return (OperationSyncer)this.GetValue(OperationSyncerProperty);
            }

            internal set
            {
                this.SetValue(OperationSyncerProperty, value);
            }
        }

        /// <summary>
		/// Get or sets map provider.
		/// </summary>
		public MapProviderBase Provider
        {
            get
            {
                return (MapProviderBase)this.GetValue(ProviderProperty);
            }

            set
            {
                this.SetValue(ProviderProperty, value);
            }
        }

        /// <summary>
		/// Gets or sets the mode of mouse wheel.
		/// </summary>
		public MouseWheelBehavior MouseWheelMode
        {
            get
            {
                return (MouseWheelBehavior)this.GetValue(MouseWheelModeProperty);
            }

            set
            {
                this.SetValue(MouseWheelModeProperty, value);
            }
        }

        /// <summary>
		/// Gets or sets value which specify mouse dragging behaviour when SHIFT key is pressed.
		/// </summary>
		public MouseDragBehavior MouseShiftDragMode
        {
            get
            {
                return (MouseDragBehavior)this.GetValue(MouseShiftDragModeProperty);
            }

            set
            {
                this.SetValue(MouseShiftDragModeProperty, value);
            }
        }

        /// <summary>
		/// Gets or sets Mouse Drag Mode property.
		/// </summary>
		public MouseDragBehavior MouseDragMode
        {
            get
            {
                return (MouseDragBehavior)this.GetValue(MouseDragModeProperty);
            }

            set
            {
                this.SetValue(MouseDragModeProperty, value);
            }
        }

        public ISpatialReference SpatialReference
        {
            get
            {
                if (this.spatialReference == null)
                {
                    this.spatialReference = new MercatorProjection();
                }

                return this.spatialReference;
            }
            set
            {
                this.spatialReference = value;
            }
        }
        public bool IsInitializeCompleletd { get; set; }

        #endregion

        #region Event
        
        /// <summary>
        /// Event occurs when initialization of the map control is completed.
        /// </summary>
        public event EventHandler InitializeCompleted;

        #endregion

        #region InternelProperty

        internal MouseControl MouseControl { get; set; }
        internal MultiScaleImage MultiScaleImage { get; set; }

        /// <summary>
		/// Gets or sets the Logical (0->1) top left of the Map.
		/// </summary>
		internal Point LogicalOrigin
        {
            get
            {
                return this.logicalOrigin;
            }
            set
            {
                Point origin = value;
                double width = this.viewportPixelWidth;
                origin = new Point(Math.Round(origin.X * width) / width, Math.Round(origin.Y * width) / width);
                this.MultiScaleImage.ViewportOrigin = origin;
                this.logicalOrigin = origin;
            }
        }
        internal double viewportPixelWidth;
        internal double viewportWidth;

        #endregion

        #region PrivateProperty

        Point logicalOrigin;
        ISpatialReference spatialReference;
        bool spatialReferenceInitialized;
        bool templateInitialized;

        /// <summary>
		/// Total pixel size of the earth surface per zoom level.
		/// </summary>
		Size[] sufaceTotalSize = null;

        #endregion

        /// <summary>
        /// Initializes static members of the RadMap class.
        /// </summary>
        [Description("Static class initializer.")]
        static Map()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Map), new FrameworkPropertyMetadata(typeof(Map)));
        }

        public Map()
        {
            this.OperationSyncer = new OperationSyncer();
            this.SizeChanged += Map_SizeChanged;
        }

        public override void OnApplyTemplate()
        {
            this.MouseControl = this.GetTemplateChild(MouseControlPartName) as MouseControl;
            this.MultiScaleImage = this.GetTemplateChild(MultiScaleImagePartName) as MultiScaleImage;
            base.OnApplyTemplate();
            this.templateInitialized = true;
            this.Initialize();
        }

        #region Internel

        internal void SetZoom(int zoomLevel)
        {
            ViewportHelper.SetZoomLevel(this, zoomLevel);
        }

        internal void SetZoomToPoint(Point point, int zoomAdjustment)
        {
            ViewportHelper.SetZoomToPoint(this, point, zoomAdjustment);
        }

        internal void SetDrag(Point point, Point origin, Location lastMapCenter)
        {
            ViewportHelper.SetDrag(this, point, origin, lastMapCenter);
        }

        internal bool SetMouseWheel(double delta, Point point)
        {
            return ViewportHelper.SetMouseWheel(this, delta, point);
        }

        internal void Initialize()
        {
            if (!this.templateInitialized) return;

            this.IsInitializeCompleletd = false;

            if(this.Provider != null)
            {
                if(this.Provider.CurrentSource != null)
                {
                    if (this.MultiScaleImage != null)
                        this.MultiScaleImage.Source = this.Provider.CurrentSource as MultiScaleTileSource;
                }
            }

            if (!this.OperationSyncer.Syncing)
                this.OperationSyncer.Start(this);

            if (this.spatialReferenceInitialized)
            {
                this.IsInitializeCompleletd = true;
                this.OnInitializeCompleted(EventArgs.Empty);
            }

            if(this.IsInitializeCompleletd)
            {
                this.SetZoom(this.ZoomLevel);
            }
        }

        #endregion

        #region Protect

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            foreach (ElementLayer elementLayer in this.Items)
            {
                if (elementLayer != null)
                {
                    Rect finalRect = new Rect(new Point(0, 0), arrangeBounds);
                    elementLayer.Arrange(finalRect);
                }
            }

            return base.ArrangeOverride(arrangeBounds);
        }

        protected virtual void OnSpatialReferenceChanged(ISpatialReference OldSpatialReference, ISpatialReference newSpatialReference)
        {
            if (newSpatialReference != null)
            {
                this.SpatialReference = this.Provider.SpatialReference;
                this.spatialReferenceInitialized = true;
                this.sufaceTotalSize = new Size[this.MaxZoomLevel];
                for (int zoomLevel = 1; zoomLevel <= this.MaxZoomLevel; zoomLevel++)
                {
                    // Canvas size
                    // map width = map height = tileSize * 2^level pixels
                    Size totalSize = new Size()
                    {
                        Width = this.Provider.TileSize.Width * Math.Pow(2, zoomLevel),
                        Height = this.Provider.TileSize.Width * Math.Pow(2, zoomLevel)
                    };

                    this.sufaceTotalSize[zoomLevel - 1] = totalSize;
                }

                this.Initialize();
            }
            else
            {
                this.SpatialReference = null;
                this.spatialReferenceInitialized = false;
            }
        }

        #endregion

        #region private

        private static void ZoomLevelChanged(DependencyObject source, DependencyPropertyChangedEventArgs eventArgs)
        {
            Map map = source as Map;
            if (map != null)
            {
                map.SetZoom(map.ZoomLevel);
            }
        }

        private static object ZoomLevelCoerced(DependencyObject source, object value)
        {
            Map map = source as Map;
            if (map != null)
            {
                value = map.CoerceZoomLevelProperty(value);
            }

            return value;
        }

        private static void CenterChangedHandler(DependencyObject source, DependencyPropertyChangedEventArgs eventArgs)
        {
            Map map = source as Map;
            map.OnCenterChanged((Location)eventArgs.OldValue, map.Center);
        }

        private static object CenterCoerced(DependencyObject source, object value)
        {
            Map map = source as Map;
            return value;
        }

        private static void ProviderChanged(DependencyObject source, DependencyPropertyChangedEventArgs eventArgs)
        {
            Map map = source as Map;
            if (map != null && eventArgs.OldValue != eventArgs.NewValue)
            {
                MapProviderBase oldProvider = eventArgs.OldValue as MapProviderBase;
                if (oldProvider != null)
                {
                    oldProvider.SpatialReferenceChanged -= map.NewProvider_SpatialReferenceChanged;
                }

                MapProviderBase newProvider = eventArgs.NewValue as MapProviderBase;
                if (newProvider != null)
                {
                    map.SpatialReference = newProvider.SpatialReference;
                    newProvider.SpatialReferenceChanged += map.NewProvider_SpatialReferenceChanged;
                    map.Initialize();
                }
            }
        }

        private static void ZoomLevelRangeChanged(DependencyObject source, DependencyPropertyChangedEventArgs eventArgs)
        {
            Map map = source as Map;
            if (map != null)
            {
                map.SetZoom((int)eventArgs.NewValue);
            }
        }

        private static void OnOperationSyncerPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs eventArgs)
        {
            Map map = source as Map;
            if (map != null && map.IsInitializeCompleletd)
            {
                OperationSyncer oldValue = eventArgs.OldValue as OperationSyncer;
                if (oldValue != null)
                    oldValue.Stop();

                OperationSyncer newValue = eventArgs.NewValue as OperationSyncer;
                if (newValue != null)
                    newValue.Start(map);
            }
        }

        private void Map_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.SetZoom(this.ZoomLevel);
        }

        private object CoerceZoomLevelProperty(object value)
        {
            int zoomLevel = (int)this.GetValidatedZoomLevel((int)value);
            return zoomLevel;
        }

        private void OnCenterChanged(Location oldCenter, Location newCenter)
        {
            ViewportHelper.SetLogicalOrigin(this, newCenter);
        }

        private void NewProvider_SpatialReferenceChanged(object sender, SpatialReferenceEventArgs e)
        {
            this.OnSpatialReferenceChanged(e.OldValue, e.NewValue);
        }

        private double GetValidatedZoomLevel(double zoomLevel)
        {
            if (zoomLevel < this.MinZoomLevel)
            {
                return this.MinZoomLevel;
            }
            if (zoomLevel > this.MaxZoomLevel)
            {
                return this.MaxZoomLevel;
            }
            return zoomLevel;
        }

        private void OnInitializeCompleted(EventArgs args)
        {
            if (this.InitializeCompleted != null)
                this.InitializeCompleted(this, args);
        }

        #endregion
    }
}
