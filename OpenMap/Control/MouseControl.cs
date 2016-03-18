using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace OpenMap
{
    /// <summary>
    /// This class represents the mouse control.
    /// </summary>
    public class MouseControl : ContentControl
    {
        /// <summary>
		/// Identifies the draged routed event.
		/// </summary>
		public static readonly RoutedEvent MouseDragedEvent = EventManager.RegisterRoutedEvent(
            "MouseDraged",
            RoutingStrategy.Bubble,
            typeof(MouseDragedRoutedEventHandler),
            typeof(MouseControl));

        /// <summary>
		/// Occurs when mouse draged.
		/// </summary>
		public event MouseDragedRoutedEventHandler MouseDraging
        {
            add
            {
                this.AddHandler(MouseDragedEvent, value);
            }
            remove
            {
                this.RemoveHandler(MouseDragedEvent, value);
            }
        }

        /// <summary>
		/// Identifies the start draged routed event.
		/// </summary>
		public static readonly RoutedEvent MouseDragStartedEvent = EventManager.RegisterRoutedEvent(
            "MouseDragStarted",
            RoutingStrategy.Bubble,
            typeof(MouseDragStartedRoutedEventHandler),
            typeof(MouseControl));

        /// <summary>
		/// Occurs when mouse drag started.
		/// </summary>
		public event MouseDragStartedRoutedEventHandler MouseDragStarted
        {
            add
            {
                this.AddHandler(MouseDragStartedEvent, value);
            }
            remove
            {
                this.RemoveHandler(MouseDragStartedEvent, value);
            }
        }

        /// <summary>
		/// Identifies the click routed event.
		/// </summary>
		public static readonly RoutedEvent MouseClickEvent = EventManager.RegisterRoutedEvent(
            "MouseClick",
            RoutingStrategy.Bubble,
            typeof(MouseClickRoutedEventHandler),
            typeof(MouseControl));

        /// <summary>
		/// Occurs when mouse drag started.
		/// </summary>
		public event MouseClickRoutedEventHandler MouseClick
        {
            add
            {
                this.AddHandler(MouseClickEvent, value);
            }
            remove
            {
                this.RemoveHandler(MouseClickEvent, value);
            }
        }

        /// <summary>
		/// Identifies the selected routed event.
		/// </summary>
		public static readonly RoutedEvent MouseSelectedEvent = EventManager.RegisterRoutedEvent(
            "MouseSelected",
            RoutingStrategy.Bubble,
            typeof(MouseSelectedRoutedEventHandler),
            typeof(MouseControl));

        /// <summary>
		/// Occurs when mouse drag started.
		/// </summary>
		public event MouseSelectedRoutedEventHandler MouseSelected
        {
            add
            {
                this.AddHandler(MouseSelectedEvent, value);
            }
            remove
            {
                this.RemoveHandler(MouseSelectedEvent, value);
            }
        }

        /// <summary>
		/// Identifies the selected routed event.
		/// </summary>
		public static new readonly RoutedEvent MouseWheelEvent = EventManager.RegisterRoutedEvent(
            "MouseWheel",
            RoutingStrategy.Bubble,
            typeof(MouseWheelRoutedEventHandler),
            typeof(MouseControl));

        /// <summary>
		/// Occurs when mouse drag started.
		/// </summary>
		public new event MouseWheelRoutedEventHandler MouseWheel
        {
            add
            {
                this.AddHandler(MouseWheelEvent, value);
            }
            remove
            {
                this.RemoveHandler(MouseWheelEvent, value);
            }
        }


        #region private members

        public double MouseWheelSensitivity { get; set; }

        private const long DoubleClickSpeed = 3000000;
        private double deltaAccumulator;
        private bool isMouseOver;
        private bool isMouseDown;
        private bool isMouseDrag;
        private bool isWheelEnabled;
        private bool isSelectMode;

        private Point mouseDownOrigin;
        private Point lastDragPoint;
        private Rectangle selectedBox;

        private DispatcherTimer timer;
        private bool eventsAttached;
        private Point wheelZoomPoint;

        #endregion

        /// <summary>
        /// Initializes static members of the MouseControl class.
        /// </summary>
        [Description("Static class initializer.")]
        static MouseControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MouseControl), new FrameworkPropertyMetadata(typeof(MouseControl)));
        }

        /// <summary>
        /// Initializes a new instance of the MouseControl class.
        /// </summary>
        public MouseControl()
        {
            this.MouseWheelSensitivity = 1.0;

            this.isWheelEnabled = true;

            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromTicks(DoubleClickSpeed);

            this.Loaded += new RoutedEventHandler(this.OnMouseControlLoaded);
            this.Unloaded += new RoutedEventHandler(this.OnMouseControlUnloaded);
        }

        /// <summary>
        /// Gets MouseControl Size. Height and Width of the area covered by Mouse Drag when in selection DragBehaviour mode.
        /// </summary>
        public Size SelectBoxSize
        {
            get
            {
                return new Size(this.selectedBox.Width, this.selectedBox.Height);
            }
            set
            {
                this.selectedBox.Width = value.Width;
                this.selectedBox.Height = value.Height;

                this.selectedBox.Visibility = value.IsEmpty == false ? Visibility.Visible : Visibility.Collapsed;
                this.Visibility = value.IsEmpty == false ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Gets MouseControl Location, Point X,Y for Top Left relative to the Canvas containing the MouseControl.
        /// </summary>
        public Point SelectBoxLocation
        {
            get
            {
                return new Point(Canvas.GetLeft(this.selectedBox), Canvas.GetTop(this.selectedBox));
            }
            set
            {
                Canvas.SetTop(this.selectedBox, value.Y);
                Canvas.SetLeft(this.selectedBox, value.X);
            }
        }

        /// <summary>
        /// Overridden from the FrameworkElement class.
        /// </summary>
        public override void OnApplyTemplate()
        {
            this.selectedBox = (Rectangle)GetTemplateChild("PART_PixelBox");

            // Test IsDesignTime, to help display control in blend correctly
            // if (HtmlPage.IsEnabled)

            if(System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                // Is DesignTime, so set some dummy Height & Widths for editing control in Blend
                this.Width = 200;
                this.Height = 200;
                this.selectedBox.Width = 200;
                this.selectedBox.Height = 200;
            }
            else
            {
                this.AttachEvents();
            }
        }

		/// <summary>
		/// Raises the <see cref="E:System.Windows.FrameworkElement.Initialized"/> event. This method is invoked whenever <see cref="P:System.Windows.FrameworkElement.IsInitialized"/> is set to true internally.
		/// </summary>
		/// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs"/> that contains the event data.</param>
		[Description("Raises the E:System.Windows.FrameworkElement.Initialized event. This method is invoked whenever P:System.Windows.FrameworkElement.IsInitialized is set to true internally.")]
		protected override void OnInitialized(EventArgs e)
		{
            base.OnInitialized(e);
		}

        private void OnMouseControlLoaded(object sender, RoutedEventArgs e)
        {
            this.AttachEvents();
        }

        private void OnMouseControlUnloaded(object sender, RoutedEventArgs e)
        {
            this.DetachEvents();
        }

        private void AttachEvents()
        {
            if (!this.eventsAttached)
            {
                this.eventsAttached = true;
                this.timer.Tick += new EventHandler(this.Timer_Tick);
                this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnPreviewMouseLeftButtonDown);
                this.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnPreviewMouseLeftButtonUp);
                this.PreviewMouseMove += new MouseEventHandler(this.OnPreviewMouseMove);
                this.MouseEnter += new MouseEventHandler(this.OnMouseEnter);
                this.MouseLeave += new MouseEventHandler(this.OnMouseLeave);
            }
        }

        private void DetachEvents()
        {
            if (this.eventsAttached)
            {
                this.eventsAttached = false;
                this.timer.Tick -= new EventHandler(this.Timer_Tick);
                this.PreviewMouseLeftButtonDown -= new MouseButtonEventHandler(this.OnPreviewMouseLeftButtonDown);
                this.PreviewMouseLeftButtonUp -= new MouseButtonEventHandler(this.OnPreviewMouseLeftButtonUp);
                this.PreviewMouseMove -= new MouseEventHandler(this.OnPreviewMouseMove);
                this.MouseEnter -= new MouseEventHandler(this.OnMouseEnter);
                this.MouseLeave -= new MouseEventHandler(this.OnMouseLeave);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.timer.Stop();
            this.RaiseEvent(new MouseClickRoutedEventArgs(MouseClickEvent, this.mouseDownOrigin));
        }

        /// <summary>
        /// The MouseEnter event handler.
        /// </summary>
        /// <param name="sender">self control.</param>
        /// <param name="e">Event arguments.</param>
        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            this.isMouseOver = true;
        }

        /// <summary>
        /// The MouseLeave event handler.
        /// </summary>
        /// <param name="sender">self control.</param>
        /// <param name="e">Event arguments.</param>
        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            this.isMouseOver = false;
            this.isMouseDown = false;
            this.isMouseDrag = false;
            if (!this.SelectBoxSize.IsEmpty)
                this.SelectBoxSize = new Size();
        }

        /// <summary>
        /// The MouseMove event handler.
        /// </summary>
        /// <param name="sender">RadMap control.</param>
        /// <param name="e">Event arguments.</param>
        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            var size = new Size(this.ActualWidth, this.ActualHeight);
            if (size.Width > 0 && size.Height > 0)
            {
                Point point = e.GetPosition((UIElement)sender);

                if (!this.isMouseDrag && this.isMouseDown && !this.isSelectMode)
                {
                    this.isMouseDrag = true;
                    this.RaiseEvent(new MouseDragStartedRoutedEventArgs(MouseDragStartedEvent, point));
                    this.lastDragPoint = point;
                }

                if (this.isMouseDrag && !this.lastDragPoint.EqualPoints(point))
                { 
                    this.RaiseEvent(new MouseDragedRoutedEventArgs(MouseDragedEvent, this.lastDragPoint, point));
                    this.lastDragPoint = point;
                }

                if(this.isSelectMode)
                {
                    this.SelectBoxLocation = new Point(Math.Min(mouseDownOrigin.X, point.X), Math.Min(mouseDownOrigin.Y, point.Y));
                    this.SelectBoxSize = new Size(Math.Abs(this.mouseDownOrigin.X - point.X), Math.Abs(this.mouseDownOrigin.Y - point.Y));
                }
            }
        }

        /// <summary>
        /// The MouseLeftButtonUp event handler.
        /// </summary>
        /// <param name="sender">RadMap control.</param>
        /// <param name="e">Event arguments.</param>
        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.isMouseDown)
            {
                Point point = e.GetPosition((UIElement)sender);
                if (!this.isMouseDrag && !this.isSelectMode)
                {
                    this.mouseDownOrigin = point;
                    this.timer.Start();
                }

                if(this.isSelectMode)
                {
                    this.RaiseEvent(new MouseSelectedRoutedEventArgs(MouseSelectedEvent, new Rect(this.mouseDownOrigin, point)));
                }
            }

            this.SelectBoxSize = new Size();
            this.isMouseDown = false;
            this.isSelectMode = false;
            this.isMouseDrag = false;
            this.isWheelEnabled = true;
        }

        /// <summary>
        /// The MouseLeftButtonDown event handler.
        /// </summary>
        /// <param name="sender">RadMap control.</param>
        /// <param name="e">Event arguments.</param>
        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.isWheelEnabled = false;
            this.timer.Stop();

            DependencyObject originalSource = e.OriginalSource as DependencyObject;
            Point point = e.GetPosition((UIElement)sender);

            this.isSelectMode = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;

            this.isMouseDown = true;
            this.mouseDownOrigin = point;
            this.isMouseDrag = false;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            Point point = e.GetPosition(this);

            if (this.isMouseOver == false)
            {
                e.Handled = true;
            }

            if (this.isWheelEnabled == false)
            {
                e.Handled = true;
            }

            double mouseWheelDelta = 120d / this.MouseWheelSensitivity;

            double delta = e.Delta;

            if (Math.Abs(this.wheelZoomPoint.X - point.X) <= 7
                && Math.Abs(this.wheelZoomPoint.Y - point.Y) <= 7
                && (this.deltaAccumulator == 0 || Math.Sign(delta) == Math.Sign(this.deltaAccumulator)))
            {
                delta += this.deltaAccumulator;
                this.deltaAccumulator = delta;
            }
            else
            {
                this.deltaAccumulator = delta;
            }

            this.deltaAccumulator = this.deltaAccumulator % mouseWheelDelta;
            this.wheelZoomPoint = point;

            delta = (int)(delta / mouseWheelDelta);

            this.RaiseEvent(new MouseWheelRoutedEventArgs(MouseWheelEvent, point, delta));
            base.OnMouseWheel(e);
        }
    }
}
