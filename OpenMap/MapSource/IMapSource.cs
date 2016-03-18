using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenMap
{
    /// <summary>
	/// Interface which must be implemented by all map sources.
	/// Map source provides methods and properties which can be used by
	/// tile layer to read images from the specific location.
	/// </summary>
	public interface IMapSource
    {
        /// <summary>
        /// Occurs when initialization of the map source is completed.
        /// </summary>
        event EventHandler InitializeCompleted;

		/// <summary>
		/// Gets or sets the CacheStorage property.
		/// </summary>
		ICacheStorage CacheStorage
		{
			get;
			set;
		}

        /// <summary>
        /// Gets or sets culture.
        /// </summary>
        CultureInfo Culture
        {
            get;
            set;
        }

		/// <summary>
		/// Gets or sets the IsTileCachingEnabled property.
		/// </summary>
		bool IsTileCachingEnabled
		{
			get;
			set;
		}

        /// <summary>
        /// Gets or sets the opacity factor.
        /// </summary>
        double Opacity
        {
            get;
            set;
        }

        /// <summary>
        /// Gets unique identifier of the map source.
        /// </summary>
        string UniqueId
        {
            get;
        }

        /// <summary>
        /// Initialize map source.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Indicates whether specified tile level is supported.
        /// </summary>
        /// <param name="level">Tile level.</param>
        /// <returns>true if tile level is supported. false otherwise.</returns>
        bool IsLevelSupported(int level);
    }
}
