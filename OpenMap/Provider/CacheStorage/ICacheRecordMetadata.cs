using System;

namespace OpenMap
{
	/// <summary>
	/// A generic map cache storage.
	/// </summary>
	public partial class MemoryCache
	{
		/// <summary>
		/// The CacheRecord metadata interface.
		/// </summary>
		protected interface ICacheRecordMetadata
		{
			/// <summary>
			/// File length.
			/// </summary>
			long FileStorageLength
			{
				get;
			}

			/// <summary>
			/// File name.
			/// </summary>
			string FileName
			{
				get;
			}

			/// <summary>
			/// Expires DateTime.
			/// </summary>
			DateTime Expires
			{
				get;
			}

			/// <summary>
			/// Last access DateTime.
			/// </summary>
			DateTime LastAccess
			{
				get;
			}
		}
	}
}
