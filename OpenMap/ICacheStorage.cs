using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenMap
{
    /// <summary>
	/// Represents the CacheStorage interface.
	/// </summary>
	public interface ICacheStorage
    {
        /// <summary>
        /// Loads file from a cache.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <returns>Stream or null if the file is absent.</returns>
        Stream Load(string fileName);

        /// <summary>
        /// Loads file from a cache.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="callback">Callback which should be called to return tile if it is available or null.</param>
        void LoadAsync(string fileName, Action<byte[]> callback);

        /// <summary>
        /// Saves file to a cache.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="expirationDate">DateTime of expires.</param>
        /// <param name="tile">Byte array which is saved to the file.</param>
        void Save(string fileName, DateTime expirationDate, byte[] tile);
    }
}
