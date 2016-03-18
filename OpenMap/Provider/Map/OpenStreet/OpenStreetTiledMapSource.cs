using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenMap
{
    /// <summary>
	/// Open street base map source.
	/// </summary>
	public class OpenStreetTiledMapSource : TiledMapSource
    {
        /// <summary>
        /// Default max zoom level for OpenStreet Maps.
        /// </summary>
        public const int DefaultMaxZoomLevel = 18;
        internal static readonly string[] DefaultTilePathPrefixes = new string[] { "a", "b", "c" };

        private readonly Random randomizer = new Random();
        private string tileUrlFormat = null;
        private string[] tilePathPrefixes = null;

        /// <summary>
        /// Initializes a new instance of the OpenStreetTiledMapSource class.
        /// </summary>
        /// <param name="tileUrlFormat">Format string for the tile renderer.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#")]
        protected OpenStreetTiledMapSource(string tileUrlFormat)
            : base(1, DefaultMaxZoomLevel, 256, 256)
        {
            this.SetValiables(tileUrlFormat, DefaultTilePathPrefixes);
        }

        /// <summary>
        /// Initializes a new instance of the OpenStreetTiledMapSource class.
        /// </summary>
        /// <param name="tileUrlFormat">Format string for the tile renderer.</param>
        /// <param name="tilePathPrefixes">Path prefixes for tile URL.</param>
        /// <param name="maxZoomLevel">Optional max zoom level. Default value for OSM is 18.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#")]
        protected OpenStreetTiledMapSource(string tileUrlFormat, string[] tilePathPrefixes, int maxZoomLevel = DefaultMaxZoomLevel)
            : base(1, maxZoomLevel, 256, 256)
        {
            this.SetValiables(tileUrlFormat, tilePathPrefixes);
        }

        internal string SourceFormat
        {
            get
            {
                return this.tileUrlFormat;
            }
        }

        internal string[] SourcePrefixes
        {
            get
            {
                return this.tilePathPrefixes;
            }
        }

        internal int SourceMaxZoomLevel
        {
            get
            {
                return this.MaxZoomLevel;
            }
        }

        /// <summary>
        /// Initialize provider.
        /// </summary>
        public override void Initialize()
        {
            // Raise provider intialized event.
            this.RaiseInitializeCompleted();
        }

        /// <summary>
        /// Gets the image URI.
        /// </summary>
        /// <param name="tileLevel">Tile level.</param>
        /// <param name="tilePositionX">Tile X.</param>
        /// <param name="tilePositionY">Tile Y.</param>
        /// <returns>URI of image.</returns>
        protected override Uri GetTile(int tileLevel, int tilePositionX, int tilePositionY)
        {
            int zoomLevel = ConvertTileToZoomLevel(tileLevel);

            string url = this.tileUrlFormat;
            string prefix = this.GetTilePrefix(tilePositionX, tilePositionY);

            url = ProtocolHelper.SetScheme(url);
            url = url.Replace("{prefix}", prefix);
            url = url.Replace("{zoom}", zoomLevel.ToString(CultureInfo.InvariantCulture));
            url = url.Replace("{x}", tilePositionX.ToString(CultureInfo.InvariantCulture));
            url = url.Replace("{y}", tilePositionY.ToString(CultureInfo.InvariantCulture));

            return new Uri(url);
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
		protected override bool IsValidCacheUri(int tileLevel, int tilePositionX, int tilePositionY, Uri uri)
		{
			Uri tileUri = this.GetTile(tileLevel, tilePositionX, tilePositionY);
			Regex regEx = new Regex(@"http\:\/\/[^\.]+\.");

			return tileUri != null
				&& regEx.Replace(uri.OriginalString, @"http://") == regEx.Replace(tileUri.OriginalString, @"http://");
		}

        private string GetTilePrefix(int tilePositionX, int tilePositionY)
        {
            string prefix = string.Empty;
            if (this.tilePathPrefixes.Length > 0)
            {
                if (this.tilePathPrefixes.Length < 5)
                {
                    int index = 0;
                    switch (this.tilePathPrefixes.Length)
                    {
                        case 2:
                            index = ((tilePositionX & 1) + (tilePositionY & 1)) & 1;
                            break;

                        case 3:
                            index = tilePositionY % 3;
                            if ((tilePositionX & 1) == 1)
                            {
                                index = 2 - index;
                            }
                            break;

                        case 4:
                            index = ((tilePositionX & 1) << 1) + (tilePositionY & 1);
                            break;
                    }

                    prefix = this.tilePathPrefixes[index];
                }
                else
                {
                    // Randomize to using different OSM Servers based on URL prefix
                    prefix = this.tilePathPrefixes[this.randomizer.Next(this.tilePathPrefixes.Length)];
                }
            }

            return prefix;
        }

        private void SetValiables(string format, string[] prefixes)
        {
            this.tileUrlFormat = format;
            this.tilePathPrefixes = prefixes;
        }
    }
}
