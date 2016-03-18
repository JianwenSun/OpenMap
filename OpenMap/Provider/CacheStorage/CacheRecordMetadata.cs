using System;

namespace OpenMap
{
	/// <summary>
	/// A generic map cache storage.
	/// </summary>
	public partial class MemoryCache
	{
		/// <summary>
		/// The CacheRecord metadata class.
		/// </summary>
		protected class CacheRecordMetadata : ICacheRecordMetadata
		{
			private string fileName;
			private long fileLength;
			private DateTime expires;
			private DateTime lastAccess;

			/// <summary>
			/// Initializes a new instance of the CacheRecordMetadata class.
			/// </summary>
			/// <param name="name">File name.</param>
			/// <param name="fileExpires">Expires.</param>
			/// <param name="length">File length.</param>
			/// <param name="lastAccessDateTime">Last access.</param>
			public CacheRecordMetadata(string name, DateTime fileExpires, long length, DateTime lastAccessDateTime)
			{
				this.fileName = name;
				this.expires = fileExpires;
				this.fileLength = length;
				this.lastAccess = lastAccessDateTime;
			}

			/// <summary>
			/// File length.
			/// </summary>
			public long FileStorageLength
			{
				get
				{
					return this.fileLength;
				}
			}

			/// <summary>
			/// File name.
			/// </summary>
			public string FileName
			{
				get
				{
					return this.fileName;
				}
			}

			/// <summary>
			/// Expires DateTime.
			/// </summary>
			public DateTime Expires
			{
				get
				{
					return this.expires;
				}
			}

			/// <summary>
			/// Last access DateTime.
			/// </summary>
			public DateTime LastAccess
			{
				get
				{
					return this.lastAccess;
				}
			}
		}
	}
}
