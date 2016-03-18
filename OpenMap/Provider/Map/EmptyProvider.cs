using System;
using System.Collections.Generic;
using System.Windows;

namespace OpenMap
{
    /// <summary>
    /// Map provider which don't show any map images.
    /// It can be used when we need not to show real map images on the control,
    /// but only some geometries given in the geographical coordinates.
    /// </summary>
	public class EmptyProvider : MapProviderBase
    {
        /// <summary>
        /// Identifies the <see cref="DistanceUnit"/> DistanceUnit dependency property.
        /// </summary>
        public static readonly DependencyProperty ProjectionProperty = DependencyProperty.Register("Projection",
            typeof(ISpatialReference),
            typeof(EmptyProvider),
            new PropertyMetadata());

        /// <summary>
        /// Initializes a new instance of the EmptyProvider class.
        /// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public EmptyProvider()
            : base()
        {
            this.Projection = new MercatorProjection();

			EmptyTileMapSource source = new EmptyTileMapSource();
			this.MapSources.Add(source.UniqueId, source);
		}

		/// <summary>
		/// Returns the SpatialReference for the map provider.
		/// </summary>
		public override ISpatialReference SpatialReference
		{
			get
			{
                if (this.Projection == null)
                {
                    return new MercatorProjection();
                }

                return this.Projection;
			}
		}

		/// <summary>
		/// Gets spatial reference.
		/// </summary>
        public ISpatialReference Projection
		{
            get
            {
                return (ISpatialReference)this.GetValue(ProjectionProperty);
            }

            set
            {
                this.SetValue(ProjectionProperty, value);
            }
        }
	}
}
