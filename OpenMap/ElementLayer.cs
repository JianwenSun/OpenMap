
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace OpenMap
{
    /// <summary>
    /// Element layer
    /// </summary>
    public class ElementLayer : MapLayer
    {
        private MapCanvas itemsPanel = null;
        /// <summary>
		/// Gets items panel.
		/// </summary>
		internal MapCanvas Canvas
        {
            get
            {
                if (this.itemsPanel == null)
                {
                    this.itemsPanel = this.FindChildByType<MapCanvas>();
                }

                return this.itemsPanel;
            }
        }

        /// <summary>
		/// Initializes static members of the InformationLayer class.
		/// </summary>
		[Description("Static class initializer.")]
        static ElementLayer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ElementLayer), new FrameworkPropertyMetadata(typeof(ElementLayer)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        /// <summary>
		/// Arrange object on the element layer.
		/// </summary>
		/// <param name="item">Object to arrange.</param>
		public void ArrangeItem(object item)
        {
            if (this.Canvas == null)
                return;

            FrameworkElement element = item as FrameworkElement;
         
            if (element != null)
            {
                this.Canvas.ArrangeItem(element);
            }
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            return base.ArrangeOverride(arrangeBounds);
        }
    }
}
