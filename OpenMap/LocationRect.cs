using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenMap
{
    /// <summary>
	/// Represents rectangle given in the geographical units.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes"), TypeConverterAttribute(typeof(LocationRectConverter))]
    public struct LocationRect
    {
        private double west;
        private double height;
        private double north;
        private double width;
        private Map map;

        private double widthKm;
        private double heightKm;

        private int index;

        /// <summary>
        /// Size of the rectangle in degrees.
        /// </summary>
        private Size geographicalSize;

        /// <summary>
        /// Initializes a new instance of the LocationRect struct.
        /// </summary>
        /// <param name="north">Latitude of the northern side of the rectangle.</param>
        /// <param name="west">Longitude of the western side of the rectangle.</param>
        /// <param name="width">Width of the rectangle given as distance unit used by RadMap control (kilometers or miles).</param>
        /// <param name="height">Height of the rectangle given as distance unit used by RadMap control (kilometers or miles).</param>
        public LocationRect(double north, double west, double width, double height)
        {
            this.north = north;
            this.west = west;
            this.width = width;
            this.height = height;
            this.geographicalSize = Size.Empty;
            this.map = null;
            this.widthKm = double.NaN;
            this.heightKm = double.NaN;
            this.index = 0;
        }

        /// <summary>
        /// Initializes a new instance of the LocationRect struct.
        /// </summary>
        /// <param name="location1">First corner of the rectangle.</param>
        /// <param name="location2">Second corner of the rectangle. </param>
        public LocationRect(Location location1, Location location2)
        {
            Location northEast = new Location();
            Location southWest = new Location();

            northEast.Latitude = Math.Max(location1.Latitude, location2.Latitude);
            northEast.Longitude = Math.Max(location1.Longitude, location2.Longitude);

            southWest.Latitude = Math.Min(location1.Latitude, location2.Latitude);
            southWest.Longitude = Math.Min(location1.Longitude, location2.Longitude);

            double locWidth = northEast.Longitude - southWest.Longitude;
            double locHeight = northEast.Latitude - southWest.Latitude;

            this.west = southWest.Longitude;
            this.north = northEast.Latitude;

            this.geographicalSize = new Size(locWidth, locHeight);
            this.map = null;

            this.widthKm = double.NaN;
            this.heightKm = double.NaN;
            this.width = double.NaN;
            this.height = double.NaN;
            this.index = 0;
        }

        /// <summary>
        /// Gets or sets the MapControl.
        /// All calculated properties (like Northwest or Southeast) of this
        /// structure are calculated using setting of this map control.
        /// </summary>
        public Map MapControl
        {
            get
            {
                return this.map;
            }

            set
            {
                Map oldValue = this.map;
                this.map = value;
            }
        }

        /// <summary>
        /// Gets geographical center of the rectangle.
        /// </summary>
        public Location Center
        {
            get
            {
                return new Location((this.North + this.South) / 2.0d, (this.West + this.East) / 2.0d);
            }
        }

        /// <summary>
        /// Gets the longitude of the eastern side of the rectangle. 
        /// </summary>
        public double East
        {
            get
            {
                if (double.IsNegativeInfinity(this.west))
                {
                    return double.NegativeInfinity;
                }
                else
                {
                    return this.west + this.geographicalSize.Width;
                }
            }
        }

        /// <summary>
        /// Gets geographical size of the location rectangle in degrees.
        /// </summary>
        public Size GeoSize
        {
            get
            {
                return this.geographicalSize;
            }
        }

        /// <summary>
        /// Gets or sets height of the rectangle given as distance unit used by RadMap control (kilometers or miles).
        /// </summary>
        public double Height
        {
            get
            {
                return this.height;
            }

            set
            {
                this.height = value;
            }
        }

        /// <summary>
        /// Gets value which indicates that given geographical rectangle is empty.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return this.East == this.West && this.North == this.South;
            }
        }

        /// <summary>
        /// Gets or sets the latitude of the northern side of the rectangle. 
        /// </summary>
        public double North
        {
            get
            {
                return this.north;
            }

            set
            {
                this.north = value;
            }
        }

        /// <summary>
        /// Gets location of the northeast corner of the rectangle. 
        /// </summary>
        public Location Northeast
        {
            get
            {
                return new Location(this.North, this.East);
            }
        }

        /// <summary>
        /// Gets location of the northwest corner of the rectangle. 
        /// </summary>
        public Location Northwest
        {
            get
            {
                return new Location(this.North, this.West);
            }
        }

        /// <summary>
        /// Gets the latitude of the southern side of the rectangle. 
        /// </summary>
        public double South
        {
            get
            {
                if (double.IsNegativeInfinity(this.north))
                {
                    return double.NegativeInfinity;
                }
                else
                {
                    return this.north - this.geographicalSize.Height;
                }
            }
        }

        /// <summary>
        /// Gets location of the southeast corner of the rectangle. 
        /// </summary>
        public Location Southeast
        {
            get
            {
                return new Location(this.South, this.East);
            }
        }

        /// <summary>
        /// Gets location of the southwest corner of the rectangle. 
        /// </summary>
        public Location Southwest
        {
            get
            {
                return new Location(this.South, this.West);
            }
        }

        /// <summary>
        /// Gets unique region ID.
        /// </summary>
        public int UniqueId
        {
            get
            {
                string key = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{0}, {1}, {2}",
                    this.Northwest,
                    this.Southeast,
                    this.ZIndex);
                return key.GetHashCode();
            }
        }

        /// <summary>
        /// Gets view center of the rectangle.
        /// </summary>
        public Location ViewCenter
        {
            get
            {
                if (this.MapControl != null
                    && this.MapControl.Provider != null)
                {
                    Point topLeft = this.MapControl.SpatialReference.GeographicToLogical(this.Northwest);
                    Point bottomRight = this.MapControl.SpatialReference.GeographicToLogical(this.Southeast);
                    Point logicalCenter = new Point((topLeft.X + bottomRight.X) / 2.0d, (topLeft.Y + bottomRight.Y) / 2.0d);
                    return this.MapControl.SpatialReference.LogicalToGeographic(logicalCenter);
                }
                else
                {
                    return new Location((this.North + this.South) / 2.0d, (this.West + this.East) / 2.0d);
                }
            }
        }

        /// <summary>
        /// Gets or sets longitude of the western side of the rectangle. 
        /// </summary>
        public double West
        {
            get
            {
                return this.west;
            }

            set
            {
                this.west = value;
            }
        }

        /// <summary>
        /// Gets or sets width of the rectangle given as distance unit used by RadMap control (kilometers or miles)..
        /// </summary>
        public double Width
        {
            get
            {
                return this.width;
            }

            set
            {
                this.width = value;
            }
        }

        /// <summary>
        /// Gets zoom level optimal to show this rectangle.
        /// </summary>
        public int ZoomLevel
        {
            get
            {
                if (this.MapControl != null)
                {
                    Point leftTopPoint = this.MapControl.SpatialReference.GeographicToLogical(this.Northwest);
                    Point rightBottomPoint = this.MapControl.SpatialReference.GeographicToLogical(this.Southeast);

                    double viewportWidth = rightBottomPoint.X - leftTopPoint.X;
                    double viewportHeight = rightBottomPoint.Y - leftTopPoint.Y;

                    Size tileSize = this.MapControl.Provider == null
                        ? new Size(256, 256)
                        : this.MapControl.Provider.TileSize;

                    Size viewportPixelSize = new Size(this.MapControl.MultiScaleImage.ActualWidth, this.MapControl.MultiScaleImage.ActualHeight);

                    int zoomWidth = (int)Math.Log(viewportPixelSize.Width / tileSize.Width / viewportWidth, 2d);
                    int zoomHeight = (int)Math.Log(viewportPixelSize.Height / tileSize.Height / viewportHeight, 2d);
                    int zoomLevel = Math.Min(zoomWidth, zoomHeight);

                    return zoomLevel;
                }
                else
                {
                    return 1;
                }
            }
        }

        /// <summary>
        /// Gets or sets ZIndex of the object.
        /// This property is used in the calculation of the UniqueId.
        /// </summary>
        internal int ZIndex
        {
            get
            {
                return this.index;
            }

            set
            {
                this.index = value;
            }
        }

        /// <summary>
        /// Compares two <see cref="LocationRect"/> structures for equality. 
        /// </summary>
        /// <param name="rect1">The instance of <see cref="LocationRect"/> to compare. </param>
        /// <param name="rect2">The instance of <see cref="LocationRect"/> to compare. </param>
        /// <returns>true if instances are equal; otherwise, false.</returns>
        public static bool Equals(LocationRect rect1, LocationRect rect2)
        {
            if (!double.IsNaN(rect1.height)
                && !double.IsNaN(rect1.width)
                && !double.IsNaN(rect2.height)
                && !double.IsNaN(rect2.width))
            {
                return rect1.Northwest == rect2.Northwest
                    && rect1.Width == rect2.Width
                    && rect1.Height == rect2.Height;
            }
            else if (!double.IsNaN(rect1.heightKm)
                && !double.IsNaN(rect1.widthKm)
                && !double.IsNaN(rect2.heightKm)
                && !double.IsNaN(rect2.widthKm))
            {
                return rect1.Northwest == rect2.Northwest
                       && rect1.Southeast == rect2.Southeast;
            }
            else if (rect1.MapControl != null && rect2.MapControl == null)
            {
                rect2.MapControl = rect1.MapControl;
            }
            else if (rect1.MapControl == null && rect2.MapControl != null)
            {
                rect1.MapControl = rect2.MapControl;
            }

            return rect1.Northwest == rect2.Northwest
                   && rect1.Southeast == rect2.Southeast;
        }

        /// <summary>
        /// Compares two <see cref="LocationRect"/> structures for equality. 
        /// </summary>
        /// <param name="rect1">The instance of <see cref="LocationRect"/> to compare. </param>
        /// <param name="rect2">The instance of <see cref="LocationRect"/> to compare. </param>
        /// <returns>true if instances are equal; otherwise, false.</returns>
        public static bool operator ==(LocationRect rect1, LocationRect rect2)
        {
            return rect1.Equals(rect2);
        }

        /// <summary>
        /// Compares two <see cref="LocationRect"/> structures for inequality. 
        /// </summary>
        /// <param name="rect1">The instance of <see cref="LocationRect"/> to compare. </param>
        /// <param name="rect2">The instance of <see cref="LocationRect"/> to compare. </param>
        /// <returns>true if instances are equal; otherwise, false.</returns>
        public static bool operator !=(LocationRect rect1, LocationRect rect2)
        {
            return !rect1.Equals(rect2);
        }

        /// <summary>
        /// Converts a String representation of a rectangle into the equivalent LocationRect object.
        /// </summary>
        /// <param name="source">The String representation of the Location object.</param>
        /// <returns>The equivalent Location structure.</returns>
        public static LocationRect Parse(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("source");
            }

            try
            {
                string[] elements = source.Split(',');
                if (elements.Length != 4)
                {
                    throw new FormatException("Input string was not in a correct format.");
                }

                double[] doubles = new double[4];
                for (int counter = 0; counter < 4; counter++)
                {
                    doubles[counter] = double.Parse(elements[counter], CultureInfo.InvariantCulture);
                }

                return new LocationRect(
                    doubles[0],
                    doubles[1],
                    doubles[2],
                    doubles[3]);
            }
            catch (Exception exc)
            {
                throw new FormatException("Input string was not in a correct format.", exc);
            }
        }

        /// <summary>
        /// Indicates whether the rectangle described by the LocationRect contains the specified location.
        /// </summary>
        /// <param name="location">Location to check.</param>
        /// <returns>true if location is inside rectangle. Otherwise false.</returns>
        public bool Contains(Location location)
        {
            if (location.Latitude <= this.North && location.Latitude >= this.South
                && location.Longitude >= this.West && location.Longitude <= this.East)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Detect whether location rectangle contains another one.
        /// </summary>
        /// <param name="other">Location rectangle to check.</param>
        /// <returns>true if this rectangle contains another one.</returns>
        public bool Contains(LocationRect other)
        {
            bool result = this.West <= other.West && this.North >= other.North
                && this.East >= other.East && this.South <= other.South;

            return result;
        }

        /// <summary>
        /// Compares two <see cref="LocationRect"/> structures for equality. 
        /// </summary>
        /// <param name="obj">The instance of <see cref="LocationRect"/> to compare to this instance. </param>
        /// <returns>true if instances are equal; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is LocationRect))
            {
                return false;
            }

            return Equals(this, (LocationRect)obj);
        }

        /// <summary>
        /// Gets a hash code for this <see cref="LocationRect"/> structure.
        /// </summary>
        /// <returns>A hash code for this <see cref="LocationRect"/> structure.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Detect whether location rectangle intersect with given line.
        /// </summary>
        /// <param name="location1">Line point 1.</param>
        /// <param name="location2">Line point 2.</param>
        /// <returns>true if line intersect rectangle.</returns>
        public bool IntersectWithLine(Location location1, Location location2)
        {
            bool intersect = false;

            Line line = new Line()
            {
                Start = location1,
                End = location2
            };

            double northEastLocation = line.GetPointLocation(this.Northeast);
            double northWestLocation = line.GetPointLocation(this.Northwest);
            double southEastLocation = line.GetPointLocation(this.Southeast);
            double southWestLocation = line.GetPointLocation(this.Southwest);

            if (!((northEastLocation < 0 && northWestLocation < 0 && southEastLocation < 0 && southWestLocation < 0)
                || (northEastLocation > 0 && northWestLocation > 0 && southEastLocation > 0 && southWestLocation > 0)))
            {
                if (!((location1.Longitude > this.East && location2.Longitude > this.East)
                    || (location1.Longitude < this.West && location2.Longitude < this.West)
                    || (location1.Latitude > this.North && location2.Latitude > this.North)
                    || (location1.Latitude < this.South && location2.Latitude < this.South)))
                {
                    intersect = true;
                }
            }

            return intersect;
        }

        /// <summary>
        /// Creates a String representation of this <see cref="LocationRect"/> object.
        /// </summary>
        /// <returns>A String containing the Latitude and Longitude values of this <see cref="LocationRect"/> object.</returns>
        public override string ToString()
        {
            return this.ToString(CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Creates a String representation of this <see cref="LocationRect"/> object. 
        /// </summary>
        /// <param name="provider">The culture-specific formatting information. </param>
        /// <returns>A String containing the Latitude and Longitude values of this <see cref="LocationRect"/> object.</returns>
        public string ToString(IFormatProvider provider)
        {
            NumberFormatInfo formatInfo = provider.GetFormat(typeof(NumberFormatInfo)) as NumberFormatInfo;
            string listSeparator = formatInfo == null || formatInfo.NumberDecimalSeparator != "," ? "," : ";";
            return this.North.ToString(provider)
                   + listSeparator + this.West.ToString(provider)
                   + listSeparator + this.Width.ToString(provider)
                   + listSeparator + this.Height.ToString(provider);
        }

        /// <summary>
        /// Represents line between 2 geographical locations.
        /// This class is necessary to calculate intersections.
        /// </summary>
        private class Line
        {
            /// <summary>
            /// Gets or sets line start point location.
            /// </summary>
            public Location Start
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets line end point location.
            /// </summary>
            public Location End
            {
                get;
                set;
            }

            /// <summary>
            /// Get location of the point relative to the line.
            /// </summary>
            /// <param name="point">Point.</param>
            /// <returns>Location relative to the line: 0 - lie on the line, &gt;0 - is "above" the line, &lt;0 - is "below" the line.</returns>
            public double GetPointLocation(Location point)
            {
                return ((this.End.Latitude - this.Start.Latitude) * point.Longitude)
                    + ((this.Start.Longitude - this.End.Longitude) * point.Latitude)
                    + ((this.End.Longitude * this.Start.Latitude) - (this.Start.Longitude * this.End.Latitude));
            }
        }
    }
}
